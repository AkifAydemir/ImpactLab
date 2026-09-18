#!/usr/bin/env python3
"""Portable numerical benchmark/reference-contract integrity audit for ImpactLab.

This audit does not compile or execute the C# numerical implementation. It checks
that canonical benchmark fixtures, catalog registration, analytical source anchors,
metric/tolerance semantics and verification tests remain internally consistent.
"""
from __future__ import annotations

import argparse
import json
import math
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

    def ok(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "passed", detail))

    def fail(self, name: str, detail: str) -> None:
        self.results.append(CheckResult(name, "failed", detail))

    @property
    def passed(self) -> bool:
        return all(x.status == "passed" for x in self.results)


def read(root: Path, rel: str) -> str:
    path = root / rel
    if not path.is_file():
        raise FileNotFoundError(rel)
    return path.read_text(encoding="utf-8")


def missing_fragments(text: str, fragments: list[str]) -> list[str]:
    """Compare semantic source anchors without depending on formatter whitespace."""
    compact_text = re.sub(r"\s+", "", text)
    return [
        fragment
        for fragment in fragments
        if fragment not in text and re.sub(r"\s+", "", fragment) not in compact_text
    ]


def load_fixture(root: Path, audit: Audit) -> list[dict[str, object]]:
    rel = "tests/ImpactLab.Core.Tests/Fixtures/v14/verification-reference.json"
    try:
        payload = json.loads(read(root, rel))
    except Exception as exc:  # noqa: BLE001
        audit.fail("verification-reference-fixture", f"Could not parse {rel}: {exc}")
        return []
    failures: list[str] = []
    if payload.get("schemaVersion") != 1:
        failures.append(f"schemaVersion={payload.get('schemaVersion')!r}, expected 1")
    rows = payload.get("benchmarks")
    if not isinstance(rows, list):
        failures.append("benchmarks must be an array")
        rows = []
    ids: list[str] = []
    allowed_reference = {"analytical", "closed-form", "constitutive-transition", "complementarity", "boundary-assembly"}
    for i, row in enumerate(rows):
        if not isinstance(row, dict):
            failures.append(f"benchmarks[{i}] is not an object")
            continue
        bid = row.get("id")
        if not isinstance(bid, str) or not bid:
            failures.append(f"benchmarks[{i}].id missing")
        else:
            ids.append(bid)
        if row.get("required") is not True:
            failures.append(f"{bid!r} is not required=true")
        if row.get("reference") not in allowed_reference:
            failures.append(f"{bid!r} has unsupported reference={row.get('reference')!r}")
    if len(ids) != len(set(ids)):
        failures.append(f"duplicate benchmark ids: {ids}")
    expected = [
        "finite-strain.pure-rotation",
        "finite-strain.neo-hookean-uniaxial",
        "plasticity.j2-transition",
        "contact.augmented-open-close",
        "thermal.convection-balance",
    ]
    if ids != expected:
        failures.append(f"fixture ids/order={ids}, expected={expected}")
    if failures:
        audit.fail("verification-reference-fixture", "; ".join(failures))
    else:
        audit.ok("verification-reference-fixture", "Schema v1 contains five unique required benchmark references in canonical order.")
    return [x for x in rows if isinstance(x, dict)]


def parse_expr(text: str, pattern: str, label: str) -> str:
    m = re.search(pattern, text, re.DOTALL)
    if not m:
        raise ValueError(f"could not parse {label}")
    return m.group(1)


def check_catalog(root: Path, fixture: list[dict[str, object]], audit: Audit) -> dict[str, str]:
    catalog_rel = "src/ImpactLab.Core/Verification/Benchmarks/VerificationBenchmarkCatalog.cs"
    catalog = read(root, catalog_rel)
    classes = re.findall(r"new\s+([A-Za-z_]\w*Benchmark)\s*\(\s*\)", catalog)
    expected_classes = [
        "PureRotationObjectivityBenchmark",
        "NeoHookeanUniaxialBenchmark",
        "J2ElasticPlasticTransitionBenchmark",
        "AugmentedContactOpeningBenchmark",
        "ThermalBoundaryBalanceBenchmark",
    ]
    failures: list[str] = []
    if classes != expected_classes:
        failures.append(f"catalog classes/order={classes}, expected={expected_classes}")
    class_to_id: dict[str, str] = {}
    class_to_category: dict[str, str] = {}
    for cls in classes:
        rel = f"src/ImpactLab.Core/Verification/Benchmarks/{cls}.cs"
        try:
            text = read(root, rel)
            bid = parse_expr(text, r'public\s+string\s+Id\s*=>\s*"([^"]+)"', f"{cls}.Id")
            category = parse_expr(text, r'Category\s*=>\s*VerificationBenchmarkCategory\.([A-Za-z_]+)', f"{cls}.Category")
        except Exception as exc:  # noqa: BLE001
            failures.append(f"{cls}: {exc}")
            continue
        class_to_id[cls] = bid
        class_to_category[cls] = category
    fixture_ids = [str(x.get("id")) for x in fixture]
    catalog_ids = [class_to_id.get(x, "") for x in classes]
    if fixture_ids and catalog_ids != fixture_ids:
        failures.append(f"catalog ids={catalog_ids}, fixture ids={fixture_ids}")
    expected_categories = ["Kinematics", "Hyperelasticity", "Plasticity", "Contact", "Thermal"]
    categories = [class_to_category.get(x, "") for x in classes]
    if categories != expected_categories:
        failures.append(f"categories={categories}, expected={expected_categories}")
    if failures:
        audit.fail("benchmark-catalog-reference-lineage", "; ".join(failures))
    else:
        audit.ok("benchmark-catalog-reference-lineage", "Fixture ids, five BuiltIns, source Id values and benchmark categories are one-to-one and ordered.")
    return class_to_id


def check_metric_contract(root: Path, audit: Audit) -> None:
    metric = read(root, "src/ImpactLab.Core/Verification/Benchmarks/VerificationBenchmarkMetric.cs")
    result = read(root, "src/ImpactLab.Core/Verification/Benchmarks/VerificationBenchmarkResult.cs")
    runner = read(root, "src/ImpactLab.Core/Verification/Benchmarks/VerificationBenchmarkRunner.cs")
    gate = read(root, "src/ImpactLab.Core/Verification/Benchmarks/VerificationGateEvaluator.cs")
    failures: list[str] = []
    for frag in missing_fragments(metric, [
        "Math.Abs(Observed-Reference)",
        "AbsoluteError/Math.Max(Math.Abs(Reference),1e-15)",
        "AbsoluteError<=AbsoluteTolerance || RelativeError<=RelativeTolerance",
    ]):
        failures.append(f"metric contract missing {frag!r}")
    for frag in missing_fragments(result, ["Error is null", "Metrics.Count>0", "Metrics.All(x=>x.Passed)"]):
        failures.append(f"result contract missing {frag!r}")
    for frag in missing_fragments(runner, ["VerificationBenchmarkCatalog.BuiltIns", "catch(Exception ex)", "Benchmark execution failed before evidence could be produced."]):
        failures.append(f"runner contract missing {frag!r}")
    for frag in missing_fragments(gate, ["Where(x=>!x.Passed)", "Select(x=>x.Id)", "failed.Length==0"]):
        failures.append(f"gate contract missing {frag!r}")
    if failures:
        audit.fail("metric-runner-gate-contract", "; ".join(failures))
    else:
        audit.ok("metric-runner-gate-contract", "Absolute/relative tolerances, non-empty evidence results, exception capture and failed-id gate aggregation remain explicit.")


def check_pure_rotation(root: Path, audit: Audit) -> None:
    text = read(root, "src/ImpactLab.Core/Verification/Benchmarks/PureRotationObjectivityBenchmark.cs")
    failures: list[str] = []
    required = [
        'public string Id=>"finite-strain.pure-rotation"',
        "const double a=1.0471975511965976",
        "var f=new Matrix3(c,-s,0,s,c,0,0,0,1)",
        'new("green-lagrange-norm","-",e,0,1e-10,1e-10)',
        'new("jacobian","-",det,1,1e-12,1e-12)',
        "R^T R = I and det(R)=1",
    ]
    for frag in missing_fragments(text, required):
        failures.append(f"source missing {frag!r}")
    a = 1.0471975511965976
    c, s = math.cos(a), math.sin(a)
    # Independent analytical invariants for the source rotation matrix.
    col0_norm = c * c + s * s
    col1_norm = s * s + c * c
    dot01 = c * (-s) + s * c
    det = c * c + s * s
    if max(abs(col0_norm - 1.0), abs(col1_norm - 1.0), abs(dot01), abs(det - 1.0)) > 1e-12:
        failures.append("independent rigid-rotation invariant calculation exceeded 1e-12")
    if failures:
        audit.fail("finite-strain-pure-rotation-reference", "; ".join(failures))
    else:
        audit.ok("finite-strain-pure-rotation-reference", f"Source anchors match R^T R=I/det(R)=1; independent det={det:.17g}.")


def check_neo_hookean(root: Path, audit: Audit) -> None:
    bench = read(root, "src/ImpactLab.Core/Verification/Benchmarks/NeoHookeanUniaxialBenchmark.cs")
    law = read(root, "src/ImpactLab.Core/Continuum/FiniteStrain/NeoHookeanLaw.cs")
    materials = read(root, "src/ImpactLab.Core/Materials/MaterialLibrary.cs")
    failures: list[str] = []
    for frag in missing_fragments(bench, [
        'public string Id=>"finite-strain.neo-hookean-uniaxial"',
        "const double stretch=1.2",
        "var lateral=1/Math.Sqrt(stretch)",
        "var expected=mu*(stretch*stretch-1)",
        'new("jacobian","-",k.Jacobian,1,1e-12,1e-12)',
    ]):
        failures.append(f"benchmark missing {frag!r}")
    for frag in missing_fragments(law, ["var mu=E/(2*(1+nu))", "var tau=(b-Matrix3.Identity)*mu+Matrix3.Identity*(lambda*Math.Log(j))"]):
        failures.append(f"NeoHookeanLaw missing {frag!r}")
    if missing_fragments(materials, ['"generic-steel", "Generic Structural Steel",\n7_850.0, 200e9, 0.30']):
        failures.append("MaterialLibrary generic-steel E/nu anchor drifted")
    stretch = 1.2
    lateral = 1.0 / math.sqrt(stretch)
    jacobian = stretch * lateral * lateral
    mu = 200e9 / (2.0 * (1.0 + 0.30))
    expected_tau_xx = mu * (stretch * stretch - 1.0)
    if abs(jacobian - 1.0) > 1e-12:
        failures.append(f"independent isochoric Jacobian={jacobian!r}")
    if not math.isclose(expected_tau_xx, 33_846_153_846.15384, rel_tol=1e-14, abs_tol=1e-6):
        failures.append(f"independent tau_xx={expected_tau_xx!r} unexpected")
    if failures:
        audit.fail("finite-strain-neo-hookean-reference", "; ".join(failures))
    else:
        audit.ok("finite-strain-neo-hookean-reference", f"Isochoric stretch source contract yields J={jacobian:.17g} and analytical Kirchhoff xx={expected_tau_xx:.6f} Pa for canonical generic steel.")


def check_contact(root: Path, audit: Audit) -> None:
    bench = read(root, "src/ImpactLab.Core/Verification/Benchmarks/AugmentedContactOpeningBenchmark.cs")
    law = read(root, "src/ImpactLab.Core/Continuum/Contact/AugmentedLagrangianContactLaw.cs")
    failures: list[str] = []
    for frag in missing_fragments(bench, [
        'public string Id=>"contact.augmented-open-close"',
        "Vec3.UnitZ,-1e-4,1,0,0",
        "Vec3.UnitZ,1e-4,1,0,0",
        "1e8,Vec3.Zero,1e-3",
        'new("closed-compression-positive","bool",c.normal>0?1:0,1,0,0)',
        'new("open-compression-zero","N",o.normal,0,1e-12,1e-12)',
    ]):
        failures.append(f"benchmark missing {frag!r}")
    if missing_fragments(law, ["Math.Max(0,old.NormalMultiplierN-penalty*p.GapMeters)"]):
        failures.append("augmented normal trial formula drifted")
    penalty = 1e8
    closed = max(0.0, 0.0 - penalty * (-1e-4))
    opened = max(0.0, 0.0 - penalty * (1e-4))
    if closed != 10000.0 or opened != 0.0:
        failures.append(f"independent complementarity anchors closed={closed}, open={opened}")
    if failures:
        audit.fail("contact-augmented-reference", "; ".join(failures))
    else:
        audit.ok("contact-augmented-reference", f"Zero-history augmented normal law independently gives closed={closed:.0f} N-equivalent source scalar and open={opened:.0f}.")


def check_thermal(root: Path, audit: Audit) -> None:
    bench = read(root, "src/ImpactLab.Core/Verification/Benchmarks/ThermalBoundaryBalanceBenchmark.cs")
    asm = read(root, "src/ImpactLab.Core/Thermal/ThermalBoundaryAssembler.cs")
    failures: list[str] = []
    for frag in missing_fragments(bench, [
        'public string Id=>"thermal.convection-balance"',
        "new([0],10,300,2)",
        'new("diagonal","W/K",diag[0],20,1e-12,1e-12)',
        'new("rhs","W",rhs[0],6000,1e-9,1e-12)',
    ]):
        failures.append(f"benchmark missing {frag!r}")
    for frag in missing_fragments(asm, [
        "var hA=b.HeatTransferCoefficientWPerM2K*b.AreaPerNodeM2",
        "diag[n]+=hA",
        "rhs[n]+=hA*b.AmbientTemperatureKelvin",
    ]):
        failures.append(f"ThermalBoundaryAssembler missing {frag!r}")
    h_a = 10.0 * 2.0
    rhs = h_a * 300.0
    if h_a != 20.0 or rhs != 6000.0:
        failures.append(f"independent convection anchors diag={h_a}, rhs={rhs}")
    if failures:
        audit.fail("thermal-convection-reference", "; ".join(failures))
    else:
        audit.ok("thermal-convection-reference", f"Independent h*A and h*A*T_inf calculation yields diagonal={h_a:.0f} W/K and rhs={rhs:.0f} W.")


def check_j2_completeness(root: Path, audit: Audit) -> None:
    text = read(root, "src/ImpactLab.Core/Verification/Benchmarks/J2ElasticPlasticTransitionBenchmark.cs")
    failures: list[str] = []
    for frag in missing_fragments(text, [
        'public string Id=>"plasticity.j2-transition"',
        "new LinearIsotropicHardening(250e6,1e9)",
        "new StrainTensor6(1e-5,0,0,0,0,0)",
        "new StrainTensor6(.02,0,0,0,0,0)",
        'new("elastic-yield-flag","bool",elastic.Yielded?1:0,0,0,0)',
        'new("plastic-yield-flag","bool",plastic.Yielded?1:0,1,0,0)',
        'new("plastic-eqp-positive","bool",plastic.State.EquivalentPlasticStrain>0?1:0,1,0,0)',
    ]):
        failures.append(f"J2 benchmark missing {frag!r}")
    if failures:
        audit.fail("j2-reference-completeness", "; ".join(failures))
    else:
        audit.ok("j2-reference-completeness", "J2 fixture/catalog entry still resolves to explicit below/above-yield source states; no portable constitutive execution is claimed.")


def check_tests_and_claim_boundary(root: Path, audit: Audit) -> None:
    tests = {
        "tests/ImpactLab.Core.Tests/PureRotationObjectivityBenchmarkTests.cs": ["PureRotationPasses", "PureRotationObjectivityBenchmark"],
        "tests/ImpactLab.Core.Tests/NeoHookeanUniaxialBenchmarkTests.cs": ["AnalyticalStatePasses", "NeoHookeanUniaxialBenchmark"],
        "tests/ImpactLab.Core.Tests/AugmentedContactTests.cs": ["PenetrationCreatesCompression", "AugmentedLagrangianContactLaw"],
        "tests/ImpactLab.Core.Tests/ThermalBoundaryTests.cs": ["ThermalBoundaryAssembler.AddConvection", "Assert.Equal(10,d[0],6)"],
        "tests/ImpactLab.Core.Tests/VerificationBenchmarkRunnerTests.cs": ["CanonicalBenchmarksProduceEvidence", "VerificationBenchmarkRunner.Run"],
        "tests/ImpactLab.Core.Tests/VerificationGateEvaluatorTests.cs": ["CatalogAssessmentIsDeterministic", "VerificationGateEvaluator.Evaluate"],
    }
    failures: list[str] = []
    for rel, fragments in tests.items():
        text = read(root, rel)
        for frag in missing_fragments(text, fragments):
            failures.append(f"{rel} missing {frag!r}")
    docs = {
        "docs/architecture/v12-finite-strain-objective-mechanics.md": ["engineering workbench feature", "pending repository verification and calibration"],
        "docs/architecture/v12-advanced-contact.md": ["future algorithm replacement"],
        "docs/architecture/v12-monolithic-coupling.md": ["visible terms instead of hidden staggered updates"],
    }
    for rel, fragments in docs.items():
        text = read(root, rel)
        for frag in missing_fragments(text, fragments):
            failures.append(f"{rel} missing claim-boundary fragment {frag!r}")
    if failures:
        audit.fail("compiled-test-and-claim-boundary", "; ".join(failures))
    else:
        audit.ok("compiled-test-and-claim-boundary", "Compiled-test source coverage exists while architecture docs keep numerical features bounded as replaceable/unverified engineering-workbench behavior.")


def write_output(path: Path, audit: Audit, fixture_count: int) -> None:
    payload = {
        "schemaVersion": 1,
        "generatedUtc": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "status": "passed" if audit.passed else "failed",
        "summary": {
            "requiredBenchmarkReferences": fixture_count,
            "portableExecution": False,
            "predictiveValidityClaimed": False,
        },
        "checks": [asdict(x) for x in audit.results],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    ap.add_argument("--output", type=Path, default=Path("artifacts/verification/numerical-reference-integrity.json"))
    args = ap.parse_args()
    root = args.root.resolve()
    output = args.output if args.output.is_absolute() else root / args.output
    audit = Audit()
    fixture: list[dict[str, object]] = []
    try:
        fixture = load_fixture(root, audit)
        check_catalog(root, fixture, audit)
        check_metric_contract(root, audit)
        check_pure_rotation(root, audit)
        check_neo_hookean(root, audit)
        check_contact(root, audit)
        check_thermal(root, audit)
        check_j2_completeness(root, audit)
        check_tests_and_claim_boundary(root, audit)
    except Exception as exc:  # noqa: BLE001
        audit.fail("audit-execution", repr(exc))
    write_output(output, audit, len(fixture))
    for row in audit.results:
        print(f"[{row.status.upper()}] {row.name}: {row.detail}")
    print(f"[{'PASS' if audit.passed else 'FAIL'}] numerical-reference-integrity -> {output}")
    return 0 if audit.passed else 2


if __name__ == "__main__":
    sys.exit(main())
