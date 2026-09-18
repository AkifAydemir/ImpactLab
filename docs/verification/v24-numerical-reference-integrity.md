# v24 portable numerical benchmark/reference-contract integrity

## Scope

Vol24 adds a deterministic Python-standard-library gate for the canonical numerical verification reference surface. The gate is intentionally pre-runtime: it does not compile or execute the C# solver and does not claim predictive validity, calibration, convergence, stability, accuracy, or runtime benchmark PASS.

Canonical reference input is `tests/ImpactLab.Core.Tests/Fixtures/v14/verification-reference.json`. The gate requires schema version 1, five unique required benchmark ids in canonical order, one-to-one registration in `VerificationBenchmarkCatalog.BuiltIns`, matching source `Id` and `VerificationBenchmarkCategory` values, and preserved metric/runner/gate semantics.

## Analytical/source anchors checked

- Pure rotation finite-strain reference: source rotation matrix, Green-Lagrange norm reference zero and Jacobian reference one are tied to an independent `R^T R = I` / `det(R) = 1` calculation.
- Neo-Hookean isochoric uniaxial reference: canonical stretch 1.2, lateral stretch `1/sqrt(1.2)`, `J = 1`, generic-steel `E = 200e9 Pa` and `nu = 0.30`, and `mu * (lambda_x^2 - 1)` are checked independently. The resulting analytical Kirchhoff xx reference is 33,846,153,846.153839 Pa.
- Augmented open/close contact reference: zero-history normal multiplier law and +/-1e-4 m gaps with penalty 1e8 independently produce a closed scalar 10,000 and open scalar 0.
- Convection boundary reference: `h = 10 W/m2K`, area 2 m2 and ambient 300 K independently produce diagonal `h*A = 20 W/K` and RHS `h*A*T_inf = 6000 W`.
- J2 transition remains part of fixture/catalog completeness with explicit below/above-yield source states; no portable constitutive execution is claimed.

Compiled xUnit sources for the benchmark runner/gate and the finite-strain/contact/thermal contracts remain present for the first SDK-capable execution. Architecture documents continue to identify the numerical implementation as an engineering-workbench feature pending verification/calibration or future algorithm replacement.

## Executed evidence

The final candidate portable numerical-reference audit passed. Three deliberate negative repositories failed closed with exit 2:

1. canonical fixture id `finite-strain.pure-rotation` drifted;
2. Neo-Hookean analytical expected-stress source anchor drifted;
3. thermal convection RHS reference drifted from 6000.

After CI wiring, the existing portable source-integrity, artifact/persistence, external-mesher, worker/extension IPC and WPF accessibility/workflow gates also passed on the candidate.

`bash eng/verify.sh` executed on 2026-09-06T18:17:02Z and exited 4 at the SDK stage because `dotnet` was unavailable. Restore, compilation, xUnit tests and compiled numerical benchmark execution did not run. This milestone therefore provides source/reference integrity evidence only and must not be interpreted as numerical validation or release readiness.
