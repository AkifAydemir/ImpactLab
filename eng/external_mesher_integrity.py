#!/usr/bin/env python3
"""Portable external-mesher wire-contract audit for ImpactLab.

This audit does not execute the .NET sample adapter or a production mesher. It validates
canonical neutral request/result fixtures and the source-level protocol invariants that can
be checked deterministically with the Python standard library.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
import re
import sys
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


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
        return all(item.status == "passed" for item in self.results)


def parse_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def finite_number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(float(value))


def vector(node: dict[str, Any]) -> tuple[float, float, float]:
    return float(node["x"]), float(node["y"]), float(node["z"])


def sub(a: tuple[float, float, float], b: tuple[float, float, float]) -> tuple[float, float, float]:
    return a[0] - b[0], a[1] - b[1], a[2] - b[2]


def dot(a: tuple[float, float, float], b: tuple[float, float, float]) -> float:
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def cross(a: tuple[float, float, float], b: tuple[float, float, float]) -> tuple[float, float, float]:
    return (
        a[1] * b[2] - a[2] * b[1],
        a[2] * b[0] - a[0] * b[2],
        a[0] * b[1] - a[1] * b[0],
    )


def signed_tetra_volume(a: tuple[float, float, float], b: tuple[float, float, float], c: tuple[float, float, float], d: tuple[float, float, float]) -> float:
    return dot(sub(b, a), cross(sub(c, a), sub(d, a))) / 6.0


def mean_ratio(points: list[tuple[float, float, float]], volume: float) -> float:
    pairs = ((0, 1), (0, 2), (0, 3), (1, 2), (1, 3), (2, 3))
    sum_sq = 0.0
    for i, j in pairs:
        delta = sub(points[i], points[j])
        sum_sq += dot(delta, delta)
    return 0.0 if sum_sq <= 0 else 12.0 * (abs(volume) ** (2.0 / 3.0)) / sum_sq


def check_protocol_schema_source(root: Path, audit: Audit) -> int | None:
    protocol = root / "src/ImpactLab.Core/Continuum/Meshing/External/ExternalMesherArtifactProtocol.cs"
    if not protocol.is_file():
        audit.fail("protocol-schema-source", "ExternalMesherArtifactProtocol.cs is missing.")
        return None
    text = protocol.read_text(encoding="utf-8")
    match = re.search(r"public\s+const\s+int\s+SchemaVersion\s*=\s*(\d+)\s*;", text)
    if not match:
        audit.fail("protocol-schema-source", "Could not parse ExternalMesherArtifactProtocol.SchemaVersion.")
        return None
    version = int(match.group(1))
    required = [
        "request.Validate();",
        "WriteIndented = true",
        "new UTF8Encoding(false)",
        "result.SchemaVersion != SchemaVersion",
        "External element references missing node id",
    ]
    missing = [fragment for fragment in required if fragment not in text]
    if missing:
        audit.fail("protocol-schema-source", f"Protocol source is missing canonical fragments: {missing}")
    else:
        audit.pass_("protocol-schema-source", f"Protocol schemaVersion={version}; deterministic JSON/write and result-admission guards are present.")
    return version


def check_request_fixture(root: Path, expected_schema: int | None, audit: Audit) -> tuple[dict[str, Any] | None, dict[str, str]]:
    path = root / "samples/v15/external-mesher/mesher-request.json"
    info = {"requestSha256": sha256(path) if path.is_file() else ""}
    failures: list[str] = []
    try:
        request = parse_json(path)
        if not isinstance(request, dict):
            failures.append("root is not an object")
            request = {}
        if expected_schema is not None and request.get("schemaVersion") != expected_schema:
            failures.append(f"schemaVersion={request.get('schemaVersion')!r}, expected {expected_schema}")
        if request.get("sourceFormat") != "neutral-fixture":
            failures.append(f"sourceFormat={request.get('sourceFormat')!r}")
        vertices = request.get("vertices")
        if not isinstance(vertices, list) or len(vertices) != 4:
            failures.append("fixture must contain exactly four vertices")
            vertices = []
        ids: list[str] = []
        for index, item in enumerate(vertices):
            if not isinstance(item, dict):
                failures.append(f"vertex[{index}] is not an object")
                continue
            vid = item.get("id")
            if not isinstance(vid, str) or not re.fullmatch(r"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", vid):
                failures.append(f"vertex[{index}] has invalid id {vid!r}")
            else:
                ids.append(vid.lower())
            for key in ("x", "y", "z", "toleranceMeters"):
                if not finite_number(item.get(key)):
                    failures.append(f"vertex[{index}].{key} is not finite")
            if finite_number(item.get("toleranceMeters")) and float(item["toleranceMeters"]) <= 0:
                failures.append(f"vertex[{index}].toleranceMeters must be positive")
        if len(ids) != len(set(ids)):
            failures.append("vertex ids are not unique")
        for key in ("edges", "faces", "bodies"):
            if not isinstance(request.get(key), list):
                failures.append(f"{key} must be an array")
        meshing = request.get("meshing")
        if not isinstance(meshing, dict):
            failures.append("meshing must be an object")
        else:
            numeric = ("targetSizeMeters", "minimumSizeMeters", "maximumSizeMeters", "curvatureFactor", "growthRate")
            for key in numeric:
                if not finite_number(meshing.get(key)):
                    failures.append(f"meshing.{key} is not finite")
            if all(finite_number(meshing.get(k)) for k in ("targetSizeMeters", "minimumSizeMeters", "maximumSizeMeters")):
                minimum = float(meshing["minimumSizeMeters"])
                target = float(meshing["targetSizeMeters"])
                maximum = float(meshing["maximumSizeMeters"])
                if minimum <= 0 or target <= 0 or maximum < minimum or not (minimum <= target <= maximum):
                    failures.append("meshing size ordering must satisfy 0 < minimum <= target <= maximum")
            if finite_number(meshing.get("growthRate")) and float(meshing["growthRate"]) < 1.0:
                failures.append("meshing.growthRate must be >= 1")
            boundary = meshing.get("boundaryLayer")
            if not isinstance(boundary, dict):
                failures.append("meshing.boundaryLayer must be an object")
        options = request.get("options")
        if not isinstance(options, dict) or options.get("fixture") != "v15":
            failures.append("options.fixture must equal 'v15'")
    except Exception as exc:  # noqa: BLE001
        audit.fail("request-fixture", f"{path}: {exc}")
        return None, info
    if failures:
        audit.fail("request-fixture", "; ".join(failures[:20]))
    else:
        audit.pass_("request-fixture", f"Request fixture schema and four-vertex neutral CAD payload are valid; sha256={info['requestSha256']}.")
    return request, info


def check_result_fixture(root: Path, expected_schema: int | None, request: dict[str, Any] | None, audit: Audit) -> tuple[dict[str, Any] | None, dict[str, str | float]]:
    path = root / "samples/v15/external-mesher/mesher-result.json"
    info: dict[str, str | float] = {"resultSha256": sha256(path) if path.is_file() else ""}
    failures: list[str] = []
    try:
        result = parse_json(path)
        if not isinstance(result, dict):
            failures.append("root is not an object")
            result = {}
        if expected_schema is not None and result.get("schemaVersion") != expected_schema:
            failures.append(f"schemaVersion={result.get('schemaVersion')!r}, expected {expected_schema}")
        if result.get("success") is not True:
            failures.append("success must be true for the canonical success fixture")
        if result.get("error") is not None:
            failures.append("error must be null for the canonical success fixture")
        nodes = result.get("nodes")
        elements = result.get("elements")
        if not isinstance(nodes, list) or len(nodes) != 4:
            failures.append("result must contain exactly four nodes")
            nodes = []
        if not isinstance(elements, list) or len(elements) != 1:
            failures.append("result must contain exactly one tetrahedron")
            elements = []
        node_by_id: dict[int, dict[str, Any]] = {}
        for index, item in enumerate(nodes):
            if not isinstance(item, dict):
                failures.append(f"node[{index}] is not an object")
                continue
            nid = item.get("id")
            if not isinstance(nid, int) or isinstance(nid, bool):
                failures.append(f"node[{index}].id must be an integer")
                continue
            if nid in node_by_id:
                failures.append(f"duplicate node id {nid}")
            node_by_id[nid] = item
            for key in ("x", "y", "z"):
                if not finite_number(item.get(key)):
                    failures.append(f"node[{index}].{key} is not finite")
            if not isinstance(item.get("partId"), str) or not item.get("partId"):
                failures.append(f"node[{index}].partId is empty")
        if sorted(node_by_id) != [0, 1, 2, 3]:
            failures.append(f"node ids {sorted(node_by_id)}, expected [0, 1, 2, 3]")
        if elements:
            element = elements[0]
            if not isinstance(element, dict):
                failures.append("element[0] is not an object")
            else:
                if element.get("id") != 0:
                    failures.append("element id must be 0")
                refs = [element.get(k) for k in ("a", "b", "c", "d")]
                if any(not isinstance(x, int) or isinstance(x, bool) for x in refs):
                    failures.append("element references must be integer node ids")
                elif len(set(refs)) != 4:
                    failures.append("element must reference four distinct node ids")
                else:
                    missing = [x for x in refs if x not in node_by_id]
                    if missing:
                        failures.append(f"element references missing node ids {missing}")
                    else:
                        points = [vector(node_by_id[x]) for x in refs]
                        volume = signed_tetra_volume(*points)
                        ratio = mean_ratio(points, volume)
                        info["signedVolume"] = volume
                        info["meanRatio"] = ratio
                        if volume <= 0:
                            failures.append(f"tetrahedron signed volume must be positive; got {volume:G6}")
                        if ratio < 1e-4:
                            failures.append(f"tetrahedron mean ratio {ratio:G6} is below canonical validation threshold 1e-4")
                if element.get("partId") != "sample-cad":
                    failures.append(f"element partId={element.get('partId')!r}, expected 'sample-cad'")
        diagnostics = result.get("diagnostics")
        if not isinstance(diagnostics, dict) or diagnostics.get("adapter") != "ImpactLab.MeshAdapter.Sample" or diagnostics.get("purpose") != "wire-contract fixture":
            failures.append("diagnostics must identify ImpactLab.MeshAdapter.Sample / wire-contract fixture")

        if request and nodes:
            req_vertices = request.get("vertices", [])
            if isinstance(req_vertices, list) and len(req_vertices) == 4 and len(node_by_id) == 4:
                ordered = sorted(req_vertices, key=lambda x: str(x.get("id")))
                for index, vertex in enumerate(ordered):
                    node = node_by_id.get(index)
                    if node is None:
                        continue
                    expected = tuple(float(vertex[k]) for k in ("x", "y", "z"))
                    actual = vector(node)
                    if actual != expected:
                        failures.append(f"node {index} coordinates {actual} do not match request vertex {expected}")
                    if node.get("partId") != "sample-cad":
                        failures.append(f"node {index} partId={node.get('partId')!r}, expected 'sample-cad'")
    except Exception as exc:  # noqa: BLE001
        audit.fail("result-fixture", f"{path}: {exc}")
        return None, info
    if failures:
        audit.fail("result-fixture", "; ".join(failures[:20]))
    else:
        audit.pass_("result-fixture", f"Result fixture maps the request vertices to one positively oriented tetrahedron; sha256={info['resultSha256']}.")
    return result, info


def check_sample_adapter_source(root: Path, audit: Audit) -> None:
    path = root / "tools/ImpactLab.MeshAdapter.Sample/Program.cs"
    failures: list[str] = []
    try:
        text = path.read_text(encoding="utf-8")
        required = [
            "request.SchemaVersion != ExternalMesherArtifactProtocol.SchemaVersion",
            "request.Vertices.Count != 4",
            "OrderBy(x => x.Id, StringComparer.Ordinal)",
            "new ExternalMeshElementArtifact(0, 0, 1, 2, 3, \"sample-cad\")",
            '["purpose"] = "wire-contract fixture"',
            "ExternalMesherArtifactProtocol.WriteResult",
        ]
        for fragment in required:
            if fragment not in text:
                failures.append(f"missing fragment {fragment!r}")
        architecture = (root / "docs/architecture/v15-external-mesher-and-report-packages.md").read_text(encoding="utf-8")
        if "deliberately not a production mesher" not in architecture:
            failures.append("v15 architecture doc no longer states that the sample adapter is non-production")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("sample-adapter-source-contract", "; ".join(failures[:20]))
    else:
        audit.pass_("sample-adapter-source-contract", "Sample adapter remains schema-gated, four-vertex deterministic, one-tetra and explicitly non-production.")


def check_process_adapter_source(root: Path, audit: Audit) -> None:
    path = root / "src/ImpactLab.Core/Continuum/Meshing/External/ExternalProcessTetraMesherAdapter.cs"
    failures: list[str] = []
    try:
        text = path.read_text(encoding="utf-8")
        required = [
            "ExternalMesherArtifactProtocol.WriteRequest(requestPath, request);",
            "ExternalMesherArtifactProtocol.ReadResult(resultPath);",
            "ExternalMesherArtifactProtocol.ToMesh(artifact, request.Material);",
            "ExternalMesherResultValidator.Validate(mesh, _validation);",
            "ExternalMesherArtifactProtocol.Sha256(requestPath)",
            "ExternalMesherArtifactProtocol.Sha256(resultPath)",
            "External mesher exceeded timeout",
        ]
        for fragment in required:
            if fragment not in text:
                failures.append(f"missing fragment {fragment!r}")
    except Exception as exc:  # noqa: BLE001
        failures.append(str(exc))
    if failures:
        audit.fail("process-adapter-source-contract", "; ".join(failures[:20]))
    else:
        audit.pass_("process-adapter-source-contract", "Process adapter source retains request/result hashing, timeout, admission and mesh-quality validation boundaries.")


def write_evidence(path: Path, audit: Audit, summary: dict[str, Any]) -> None:
    payload = {
        "schemaVersion": 1,
        "generatedUtc": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "status": "passed" if audit.passed else "failed",
        "summary": summary,
        "checks": [asdict(item) for item in audit.results],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=Path("artifacts/verification/external-mesher-integrity.json"))
    args = parser.parse_args()
    root = args.root.resolve()
    output = args.output if args.output.is_absolute() else root / args.output

    audit = Audit()
    schema = check_protocol_schema_source(root, audit)
    request, request_info = check_request_fixture(root, schema, audit)
    _, result_info = check_result_fixture(root, schema, request, audit)
    check_sample_adapter_source(root, audit)
    check_process_adapter_source(root, audit)

    summary: dict[str, Any] = {
        "protocolSchemaVersion": schema,
        **request_info,
        **result_info,
    }
    write_evidence(output, audit, summary)
    for result in audit.results:
        print(f"[{result.status.upper()}] {result.name}: {result.detail}")
    print(f"[{'PASS' if audit.passed else 'FAIL'}] external-mesher-integrity -> {output}")
    return 0 if audit.passed else 2


if __name__ == "__main__":
    sys.exit(main())
