#!/usr/bin/env python3
"""Portable source-integrity audit for ImpactLab.

This audit deliberately does not compile or execute C# code. It validates repository
structure and serialization contracts that can be checked without the .NET SDK.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, asdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable

EXCLUDED_DIRS = {".git", ".vs", "artifacts", "bin", "obj", "TestResults", "__pycache__"}
XML_SUFFIXES = {".csproj", ".props", ".targets", ".xaml"}


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


def canonical_files(root: Path) -> list[Path]:
    files: list[Path] = []
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        rel = path.relative_to(root)
        if any(part in EXCLUDED_DIRS for part in rel.parts):
            continue
        files.append(path)
    return sorted(files, key=lambda p: p.relative_to(root).as_posix())


def normalize_project_path(value: str) -> str:
    return value.replace("\\", "/")


def check_paths(root: Path, files: list[Path], audit: Audit) -> None:
    rels = [p.relative_to(root).as_posix() for p in files]
    folded: dict[str, list[str]] = {}
    for rel in rels:
        folded.setdefault(rel.casefold(), []).append(rel)
    collisions = [items for items in folded.values() if len(items) > 1]
    if collisions:
        audit.fail("path-uniqueness", f"Case-insensitive path collisions: {collisions[:5]}")
    else:
        audit.pass_("path-uniqueness", f"{len(rels)} canonical filesystem paths are case-insensitively unique.")

    forbidden = [
        rel for rel in rels
        if any(part in {"bin", "obj", "artifacts", "TestResults"} for part in Path(rel).parts)
    ]
    if forbidden:
        audit.fail("generated-artifacts", f"Generated/runtime paths entered canonical source: {forbidden[:10]}")
    else:
        audit.pass_("generated-artifacts", "No bin/obj/artifacts/TestResults paths are present in the canonical source set.")


def check_json(root: Path, files: list[Path], audit: Audit) -> int:
    json_files = [p for p in files if p.suffix.lower() == ".json"]
    failures: list[str] = []
    for path in json_files:
        try:
            json.loads(path.read_text(encoding="utf-8"))
        except Exception as exc:  # noqa: BLE001 - evidence should contain parser diagnostics
            failures.append(f"{path.relative_to(root).as_posix()}: {exc}")
    if failures:
        audit.fail("json-parse", "; ".join(failures[:10]))
    else:
        audit.pass_("json-parse", f"Parsed {len(json_files)} JSON documents.")
    return len(json_files)


def check_xml(root: Path, files: list[Path], audit: Audit) -> int:
    xml_files = [p for p in files if p.suffix.lower() in XML_SUFFIXES]
    failures: list[str] = []
    for path in xml_files:
        try:
            ET.parse(path)
        except Exception as exc:  # noqa: BLE001
            failures.append(f"{path.relative_to(root).as_posix()}: {exc}")
    if failures:
        audit.fail("xml-parse", "; ".join(failures[:10]))
    else:
        audit.pass_("xml-parse", f"Parsed {len(xml_files)} project/props/targets/XAML XML documents.")
    return len(xml_files)


def parse_solution_projects(solution: Path) -> list[str]:
    pattern = re.compile(r'^Project\("\{[^}]+\}"\)\s*=\s*"[^"]+",\s*"([^"]+\.csproj)"', re.MULTILINE | re.IGNORECASE)
    return [normalize_project_path(x) for x in pattern.findall(solution.read_text(encoding="utf-8-sig"))]


def check_solution(root: Path, files: list[Path], audit: Audit) -> tuple[int, int]:
    solution = root / "ImpactLab.sln"
    if not solution.is_file():
        audit.fail("solution-projects", "ImpactLab.sln is missing.")
        return 0, 0

    declared = parse_solution_projects(solution)
    declared_set = {Path(p).as_posix().casefold() for p in declared}
    missing = [p for p in declared if not (root / p).is_file()]
    canonical_projects = sorted(
        p.relative_to(root).as_posix()
        for p in files
        if p.suffix.lower() == ".csproj"
    )
    omitted = [p for p in canonical_projects if p.casefold() not in declared_set]
    duplicates = sorted({p for p in declared if declared.count(p) > 1})
    if missing or omitted or duplicates:
        audit.fail(
            "solution-projects",
            f"declared={len(declared)} missing={missing} omitted={omitted} duplicates={duplicates}",
        )
    else:
        audit.pass_("solution-projects", f"ImpactLab.sln contains all {len(declared)} canonical projects exactly once.")

    references: list[tuple[str, str]] = []
    missing_refs: list[str] = []
    for proj_rel in canonical_projects:
        proj = root / proj_rel
        try:
            tree = ET.parse(proj)
        except ET.ParseError:
            continue
        for elem in tree.iter():
            if elem.tag.split("}")[-1] != "ProjectReference":
                continue
            include = elem.attrib.get("Include", "")
            if not include:
                missing_refs.append(f"{proj_rel}: empty ProjectReference")
                continue
            target = (proj.parent / normalize_project_path(include)).resolve()
            try:
                target_rel = target.relative_to(root.resolve()).as_posix()
            except ValueError:
                missing_refs.append(f"{proj_rel}: reference escapes repository: {include}")
                continue
            references.append((proj_rel, target_rel))
            if not target.is_file():
                missing_refs.append(f"{proj_rel}: missing {target_rel}")
    if missing_refs:
        audit.fail("project-references", "; ".join(missing_refs[:20]))
    else:
        audit.pass_("project-references", f"Resolved {len(references)} ProjectReference edges inside the repository.")
    return len(declared), len(references)


def require_exact_indent(lines: list[str], token: str, indent: int, failures: list[str]) -> None:
    matches = [line for line in lines if line.lstrip() == token]
    if not matches:
        failures.append(f"missing '{token}'")
        return
    if not any(len(line) - len(line.lstrip(" ")) == indent for line in matches):
        actual = sorted({len(line) - len(line.lstrip(" ")) for line in matches})
        failures.append(f"'{token}' indentation {actual}, expected {indent}")


def check_verify_workflow(root: Path, audit: Audit) -> None:
    path = root / ".github/workflows/verify.yml"
    if not path.is_file():
        audit.fail("verify-workflow", ".github/workflows/verify.yml is missing.")
        return
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    failures: list[str] = []
    if "\t" in text:
        failures.append("tab indentation is forbidden")
    # These indentation contracts intentionally catch the Vol17 re-host YAML defect.
    for token, indent in [
        ("name: verify", 0),
        ("on:", 0),
        ("push:", 2),
        ("pull_request:", 2),
        ("workflow_dispatch:", 2),
        ("jobs:", 0),
        ("build-test:", 2),
        ("runs-on: windows-latest", 4),
        ("steps:", 4),
    ]:
        require_exact_indent(lines, token, indent, failures)
    required_fragments = [
        "actions/checkout@v7",
        "actions/setup-python@v6",
        "python eng/source_integrity.py",
        "python eng/artifact_integrity.py",
        "python eng/external_mesher_integrity.py",
        "python eng/process_ipc_integrity.py",
        "python eng/accessibility_integrity.py",
        "python eng/numerical_reference_integrity.py",
        "artifacts/verification/artifact-integrity.json",
        "artifacts/verification/external-mesher-integrity.json",
        "artifacts/verification/process-ipc-integrity.json",
        "artifacts/verification/accessibility-integrity.json",
        "artifacts/verification/numerical-reference-integrity.json",
        "actions/setup-dotnet@v6",
        "./eng/verify.ps1",
        "artifacts/verification/source-integrity.json",
        "artifacts/verification/verify-evidence.json",
    ]
    for fragment in required_fragments:
        if fragment not in text:
            failures.append(f"missing fragment '{fragment}'")
    if failures:
        audit.fail("verify-workflow", "; ".join(failures))
    else:
        audit.pass_("verify-workflow", "Workflow indentation and source/toolchain evidence wiring satisfy the canonical contract.")


def parse_explicit_enum_values(path: Path, enum_name: str) -> list[int]:
    text = path.read_text(encoding="utf-8")
    m = re.search(rf'enum\s+{re.escape(enum_name)}\s*\{{(?P<body>.*?)\}}', text, re.DOTALL)
    if not m:
        return []
    values: list[int] = []
    for assignment in re.finditer(r'\b[A-Za-z_]\w*\s*=\s*(-?\d+)\b', m.group("body")):
        values.append(int(assignment.group(1)))
    return values


def check_release_gate_enum(root: Path, audit: Audit) -> None:
    path = root / "src/ImpactLab.Core/Finalization/ReleaseGateKind.cs"
    values = parse_explicit_enum_values(path, "ReleaseGateKind") if path.is_file() else []
    if not values:
        audit.fail("release-gate-enum", "ReleaseGateKind explicit numeric values could not be parsed.")
        return
    if len(values) != len(set(values)):
        audit.fail("release-gate-enum", f"Duplicate numeric ReleaseGateKind values: {values}")
        return
    if values != sorted(values):
        audit.fail("release-gate-enum", f"ReleaseGateKind numeric values are not monotonic: {values}")
        return
    audit.pass_("release-gate-enum", f"ReleaseGateKind has {len(values)} unique monotonic explicit numeric values ({values[0]}..{values[-1]}).")


def check_verification_entrypoints(root: Path, audit: Audit) -> None:
    bash = root / "eng/verify.sh"
    ps = root / "eng/verify.ps1"
    failures: list[str] = []
    if not bash.is_file():
        failures.append("eng/verify.sh missing")
    if not ps.is_file():
        failures.append("eng/verify.ps1 missing")
    if bash.is_file():
        text = bash.read_text(encoding="utf-8")
        positions = [text.find(x) for x in ['run_stage "restore"', 'run_stage "build"', 'run_stage "tests"']]
        if any(x < 0 for x in positions) or positions != sorted(positions):
            failures.append("eng/verify.sh restore/build/tests stages are missing or out of order")
        if "environment-blocked" not in text or "verify-evidence.json" not in text:
            failures.append("eng/verify.sh evidence/environment-blocked contract missing")
    if ps.is_file():
        text = ps.read_text(encoding="utf-8")
        positions = [text.find(x) for x in ["Invoke-DotNetStage 'restore'", "Invoke-DotNetStage 'build'", "Invoke-DotNetStage 'tests'"]]
        if any(x < 0 for x in positions) or positions != sorted(positions):
            failures.append("eng/verify.ps1 restore/build/tests stages are missing or out of order")
        if "environment-blocked" not in text or "verify-evidence.json" not in text:
            failures.append("eng/verify.ps1 evidence/environment-blocked contract missing")
    if failures:
        audit.fail("verification-entrypoints", "; ".join(failures))
    else:
        audit.pass_("verification-entrypoints", "Bash/PowerShell toolchain stages remain restore -> build -> tests with environment-blocked evidence support.")


def run(root: Path) -> tuple[Audit, dict[str, int | str]]:
    audit = Audit()
    files = canonical_files(root)
    check_paths(root, files, audit)
    json_count = check_json(root, files, audit)
    xml_count = check_xml(root, files, audit)
    project_count, reference_count = check_solution(root, files, audit)
    check_verify_workflow(root, audit)
    check_release_gate_enum(root, audit)
    check_verification_entrypoints(root, audit)

    cs_files = sum(1 for p in files if p.suffix.lower() == ".cs")
    summary: dict[str, int | str] = {
        "canonicalFilesObserved": len(files),
        "csharpFilesObserved": cs_files,
        "jsonDocumentsParsed": json_count,
        "xmlDocumentsParsed": xml_count,
        "solutionProjects": project_count,
        "projectReferenceEdges": reference_count,
    }
    return audit, summary


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=Path("artifacts/verification/source-integrity.json"))
    args = parser.parse_args()

    root = args.root.resolve()
    audit, summary = run(root)
    payload = {
        "schemaVersion": 1,
        "generatedUtc": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "status": "passed" if audit.passed else "failed",
        "summary": summary,
        "checks": [asdict(x) for x in audit.results],
    }
    output = args.output
    if not output.is_absolute():
        output = root / output
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    for result in audit.results:
        print(f"[{result.status.upper()}] {result.name}: {result.detail}")
    print(f"[{'PASS' if audit.passed else 'FAIL'}] source-integrity -> {output}")
    return 0 if audit.passed else 2


if __name__ == "__main__":
    raise SystemExit(main())
