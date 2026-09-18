#!/usr/bin/env python3
"""Portable artifact/persistence integrity audit for ImpactLab.

This audit deliberately does not execute C# migrations or report renderers. It verifies
canonical fixture bytes, declared migration lineages, source-coverage references and
cross-file metadata that can be established honestly with the Python standard library.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path


@dataclass
class CheckResult:
    name: str
    status: str
    detail: str


class Audit:
    def __init__(self) -> None:
        self.results: list[CheckResult] = []

    def pass_(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "passed", detail))

    def fail(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "failed", detail))

    @property
    def passed(self) -> bool:
        return all(r.status == "passed" for r in self.results)


def sha256_hex(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def under_root(root: Path, relative: str) -> Path | None:
    candidate = (root / relative).resolve()
    try:
        candidate.relative_to(root.resolve())
    except ValueError:
        return None
    return candidate


def parse_json(path: Path) -> dict:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"Expected JSON object in {path}")
    return value


def parse_utc(value: str) -> datetime:
    text = value.replace("Z", "+00:00")
    # .NET round-trip timestamps can contain seven fractional digits; Python accepts six.
    text = re.sub(r"(\.\d{6})\d(?=[+-]\d\d:\d\d$)", r"\1", text)
    dt = datetime.fromisoformat(text)
    if dt.tzinfo is None:
        raise ValueError("timestamp is not offset-aware")
    return dt.astimezone(timezone.utc)


def check_report_golden(root: Path, audit: Audit) -> int:
    folder = root / "samples/v15/report-golden"
    manifest_path = folder / "manifest.json"
    expected_paths = {"metrics.csv", "report.html", "report.json", "report.md"}
    failures: list[str] = []
    verified = 0
    try:
        manifest = parse_json(manifest_path)
        if manifest.get("schemaVersion") != 1:
            failures.append(f"schemaVersion={manifest.get('schemaVersion')!r}, expected 1")
        rows = manifest.get("files")
        if not isinstance(rows, list):
            raise ValueError("manifest.files must be an array")
        seen: set[str] = set()
        for row in rows:
            if not isinstance(row, dict):
                failures.append("manifest.files contains a non-object row")
                continue
            rel = row.get("path")
            if not isinstance(rel, str) or not rel:
                failures.append("manifest row has invalid path")
                continue
            if rel in seen:
                failures.append(f"duplicate report artifact path {rel}")
                continue
            seen.add(rel)
            target = under_root(folder, rel)
            if target is None or target.parent != folder.resolve():
                failures.append(f"unsafe/non-sibling report artifact path {rel}")
                continue
            if not target.is_file():
                failures.append(f"missing report artifact {rel}")
                continue
            payload = target.read_bytes()
            actual_bytes = len(payload)
            actual_sha = hashlib.sha256(payload).hexdigest()
            if row.get("bytes") != actual_bytes:
                failures.append(f"{rel}: bytes {actual_bytes}, manifest {row.get('bytes')}")
            if str(row.get("sha256", "")).lower() != actual_sha:
                failures.append(f"{rel}: sha256 mismatch")
            verified += 1
        if seen != expected_paths:
            failures.append(f"artifact set {sorted(seen)}, expected {sorted(expected_paths)}")

        report = parse_json(folder / "report.json")
        if report.get("backend") != manifest.get("backendId"):
            failures.append("report.json backend does not match manifest backendId")
        try:
            if parse_utc(str(report.get("generatedUtc"))) != parse_utc(str(manifest.get("generatedUtc"))):
                failures.append("report.json generatedUtc does not match manifest generatedUtc")
        except Exception as exc:  # noqa: BLE001
            failures.append(f"generatedUtc parse/consistency failure: {exc}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))

    if failures:
        audit.fail("report-golden-package", "; ".join(failures[:20]))
    else:
        audit.pass_("report-golden-package", f"Verified {verified} report artifacts against manifest byte/SHA metadata and report identity fields.")
    return verified


def check_source_coverage(root: Path, audit: Audit) -> int:
    matrices = [root / "docs/matrices/v15-source-coverage.json", root / "docs/matrices/v16-source-coverage.json"]
    failures: list[str] = []
    refs = 0
    for matrix_path in matrices:
        try:
            matrix = parse_json(matrix_path)
            requirements = matrix.get("requirements")
            if not isinstance(requirements, list):
                raise ValueError(f"{matrix_path.name}: requirements must be an array")
            ids: set[str] = set()
            for requirement in requirements:
                if not isinstance(requirement, dict):
                    failures.append(f"{matrix_path.name}: non-object requirement")
                    continue
                rid = requirement.get("id")
                if not isinstance(rid, str) or not rid:
                    failures.append(f"{matrix_path.name}: invalid requirement id")
                    continue
                if rid in ids:
                    failures.append(f"{matrix_path.name}: duplicate requirement id {rid}")
                ids.add(rid)
                paths = requirement.get("paths")
                if not isinstance(paths, list) or not paths:
                    failures.append(f"{matrix_path.name}:{rid}: paths must be non-empty")
                    continue
                local_seen: set[str] = set()
                for rel in paths:
                    if not isinstance(rel, str) or not rel:
                        failures.append(f"{matrix_path.name}:{rid}: invalid path")
                        continue
                    if rel in local_seen:
                        failures.append(f"{matrix_path.name}:{rid}: duplicate path {rel}")
                    local_seen.add(rel)
                    target = under_root(root, rel)
                    if target is None or not target.is_file():
                        failures.append(f"{matrix_path.name}:{rid}: missing/unsafe canonical path {rel}")
                    refs += 1
            if matrix_path.name.startswith("v16") and matrix.get("sourceFeatureReview") != "ready-for-extraction":
                failures.append("v16 sourceFeatureReview is not ready-for-extraction")
        except Exception as exc:  # noqa: BLE001
            failures.append(str(exc))

    if failures:
        audit.fail("source-coverage-references", "; ".join(failures[:20]))
    else:
        audit.pass_("source-coverage-references", f"Resolved {refs} canonical path references across v15/v16 source-coverage matrices.")
    return refs


def check_migration_fixtures(root: Path, audit: Audit) -> int:
    versions = {"v9": 2, "v10": 3, "v11": 4, "v12": 5}
    types = ("scenario", "experiment", "workspace")
    failures: list[str] = []
    observed = 0
    for folder, version in versions.items():
        base = root / "tests/ImpactLab.Core.Tests/Fixtures" / folder
        for kind in types:
            path = base / f"{kind}-v{version}.json"
            if not path.is_file():
                failures.append(f"missing fixture {path.relative_to(root).as_posix()}")
                continue
            try:
                payload = parse_json(path)
                if payload.get("schemaVersion") != version:
                    failures.append(f"{path.name}: schemaVersion={payload.get('schemaVersion')!r}, expected {version}")
                declared_kind = payload.get("documentType", payload.get("kind"))
                if declared_kind != kind:
                    failures.append(f"{path.name}: kind/documentType={declared_kind!r}, expected {kind!r}")
                observed += 1
            except Exception as exc:  # noqa: BLE001
                failures.append(f"{path.name}: {exc}")
    if failures:
        audit.fail("migration-fixture-lineage", "; ".join(failures[:20]))
    else:
        audit.pass_("migration-fixture-lineage", f"Validated {observed} scenario/experiment/workspace fixtures across schema versions 2..5.")
    return observed


def parse_migration_decl(path: Path) -> tuple[str, int, int] | None:
    text = path.read_text(encoding="utf-8")
    kind = re.search(r'DocumentType\s*=>\s*"([^"]+)"', text)
    from_v = re.search(r'FromVersion\s*=>\s*(\d+)', text)
    to_v = re.search(r'ToVersion\s*=>\s*(\d+)', text)
    if not (kind and from_v and to_v):
        return None
    return kind.group(1), int(from_v.group(1)), int(to_v.group(1))


def check_document_migration_sources(root: Path, audit: Audit) -> int:
    expected = {(kind, a, b) for kind in ("scenario", "experiment", "workspace") for a, b in ((3, 4), (4, 5))}
    folder = root / "src/ImpactLab.Core/IO/Migrations"
    observed: dict[tuple[str, int, int], list[str]] = {}
    for path in folder.glob("*MigrationV*ToV*.cs"):
        decl = parse_migration_decl(path)
        if decl is not None:
            observed.setdefault(decl, []).append(path.name)
    failures: list[str] = []
    missing = sorted(expected - set(observed))
    extras = sorted(set(observed) - expected)
    duplicates = {k: v for k, v in observed.items() if len(v) != 1}
    if missing:
        failures.append(f"missing edges {missing}")
    if extras:
        failures.append(f"unexpected edges {extras}")
    if duplicates:
        failures.append(f"duplicate edges {duplicates}")

    registry = (folder / "DocumentMigrationRegistry.cs").read_text(encoding="utf-8")
    expected_classes = {
        "ScenarioMigrationV3ToV4", "ScenarioMigrationV4ToV5",
        "ExperimentMigrationV3ToV4", "ExperimentMigrationV4ToV5",
        "WorkspaceMigrationV3ToV4", "WorkspaceMigrationV4ToV5",
    }
    registered = set(re.findall(r'r\.Register\(new\s+([A-Za-z_]\w*)\(\)\)', registry))
    if registered != expected_classes:
        failures.append(f"DocumentMigrationRegistry classes {sorted(registered)}, expected {sorted(expected_classes)}")

    matrix = (folder / "MigrationMatrixCatalog.cs").read_text(encoding="utf-8")
    for fragment in ('new[] { "scenario", "experiment", "workspace" }', 'm.Add(new(type, 3, 4', 'm.Add(new(type, 4, 5'):
        if fragment not in matrix:
            failures.append(f"MigrationMatrixCatalog missing fragment {fragment!r}")

    if failures:
        audit.fail("document-migration-source-coverage", "; ".join(failures[:20]))
    else:
        audit.pass_("document-migration-source-coverage", "Document migration sources, registry and matrix cover scenario/experiment/workspace 3->4->5 exactly once.")
    return len(observed)


def check_envelope_migration_registry(root: Path, audit: Audit) -> int:
    folder = root / "src/ImpactLab.Core/Persistence/Migrations"
    registry_path = folder / "V10MigrationRegistry.cs"
    failures: list[str] = []
    expected = {
        ("experiment", 1, 2), ("experiment", 2, 3),
        ("workspace", 1, 2), ("workspace", 2, 3),
        ("scenario", 2, 3),
    }
    try:
        registry = registry_path.read_text(encoding="utf-8")
        class_names = re.findall(r'new\s+([A-Za-z_]\w*)\(\)', registry)
        observed: dict[tuple[str, int, int], list[str]] = {}
        for class_name in class_names:
            path = folder / f"{class_name}.cs"
            if not path.is_file():
                failures.append(f"registry references missing class file {class_name}.cs")
                continue
            decl = parse_migration_decl(path)
            if decl is None:
                failures.append(f"could not parse migration declaration in {class_name}.cs")
                continue
            observed.setdefault(decl, []).append(class_name)
        if set(observed) != expected:
            failures.append(f"envelope edges {sorted(observed)}, expected {sorted(expected)}")
        duplicates = {k: v for k, v in observed.items() if len(v) != 1}
        if duplicates:
            failures.append(f"duplicate envelope edges {duplicates}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
        observed = {}

    if failures:
        audit.fail("envelope-migration-registry", "; ".join(failures[:20]))
    else:
        audit.pass_("envelope-migration-registry", "V10 envelope registry has the expected five unique migration edges through schema v3.")
    return len(observed)


def write_evidence(path: Path, root: Path, audit: Audit, summary: dict[str, int]) -> None:
    payload = {
        "schemaVersion": 1,
        "generatedUtc": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "status": "passed" if audit.passed else "failed",
        "summary": summary,
        "checks": [asdict(item) for item in audit.results],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2, sort_keys=False) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parent.parent)
    parser.add_argument("--output", type=Path, default=Path("artifacts/verification/artifact-integrity.json"))
    args = parser.parse_args()
    root = args.root.resolve()
    output = args.output if args.output.is_absolute() else root / args.output

    audit = Audit()
    report_files = check_report_golden(root, audit)
    coverage_refs = check_source_coverage(root, audit)
    fixture_count = check_migration_fixtures(root, audit)
    document_edges = check_document_migration_sources(root, audit)
    envelope_edges = check_envelope_migration_registry(root, audit)
    summary = {
        "reportArtifactsVerified": report_files,
        "sourceCoverageReferencesResolved": coverage_refs,
        "migrationFixturesValidated": fixture_count,
        "documentMigrationEdgesValidated": document_edges,
        "envelopeMigrationEdgesValidated": envelope_edges,
    }
    write_evidence(output, root, audit, summary)
    for result in audit.results:
        print(f"[{result.status.upper()}] {result.name}: {result.detail}")
    print(f"[{'PASS' if audit.passed else 'FAIL'}] artifact-integrity -> {output}")
    return 0 if audit.passed else 2


if __name__ == "__main__":
    sys.exit(main())
