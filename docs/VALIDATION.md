# Validation

## Current local release-candidate result

Validated on Windows with a .NET 8 SDK:

```powershell
dotnet tool restore
dotnet tool run csharpier check .
dotnet build .\ImpactLab.sln -c Release --no-restore --nologo -v:minimal
dotnet test .\ImpactLab.sln -c Release --no-build --nologo -v:minimal
```

Result:

- release solution build: **passed**;
- compiler/analyzer output: **0 warnings, 0 errors**;
- xUnit: **111 passed, 0 failed, 0 skipped**;
- CSharpier: **1,171 files checked, no formatting drift**;
- portable integrity audits: **6 passed, 0 failed**;
- WPF startup smoke test: **passed; the process remained alive with a main-window handle for 8 seconds before the test harness stopped it**;
- interactive Quick Impact Sandbox run in the Release app: **completed**, using 20 mm mesh cells, 20 µs time step, and default remaining inputs; the app displayed 3.000 ms simulated time, 235 ms compute time, and 31 telemetry samples;
- compiled projects: `ImpactLab.Core`, `ImpactLab.App`, `ImpactLab.Worker`, `ImpactLab.ExtensionHost`, `ImpactLab.Extractor`, `ImpactLab.MeshAdapter.Sample`, and `ImpactLab.Core.Tests`.

The current [Quick Impact capture](assets/impactlab-quick-impact-release-clean.jpg) is a direct, full-window screenshot of a second completed Release-app run with a 10 mm mesh setting and visible 3D output and telemetry, not a mock-up. The initial built-in plate-impact **Solve → Results** attempt was unresponsive; the bounded repair and successful subsequent run are documented below.

After a bounded preset repair, the real Release application completed **Load Sample → Solve → Results Explorer** for `Plate impact study` using `Explicit Lattice Dynamics`. The preset now explicitly applies its 30 mm uniform mesh setting and uses a 20 µs time step over 3 ms; the earlier 10 mm meshing default had overridden the advertised cell size and made this UI demonstration unresponsive. Results Explorer opened automatically at frame `15` and displayed node rows. This flow was repeated after the optional out-of-core panel was changed to appear only when a store is attached; the panel no longer showed an unrelated empty-state message beside the ordinary result rows. Sampled damage and displacement rows in this coarse scenario were zero; completion is evidence of the workflow and data path, **not** a validated impact prediction. Quick Impact remains the more informative visual demonstration, so the earlier Results screenshot is not used as portfolio evidence.

`Directory.Build.props` keeps nullable analysis, recommended .NET analyzers, deterministic builds, and warnings-as-errors enabled. `.editorconfig` lowers only compatibility-sensitive naming/design suggestions; correctness, globalization, resource-lifetime, and cancellation diagnostics remain build-blocking.

## What the tests cover

Representative groups include:

- sparse CG/PCG/BiCGStab behavior and sparse matrix composition;
- tetrahedral geometry, structured meshing, mass assembly, and linear static continuum solving;
- central difference/Newmark integration and nonlinear convergence;
- J2 return mapping, hardening, rate/temperature adjustment, Neo-Hookean response, pure-rotation objectivity, and a uniaxial analytical benchmark;
- penalty/friction and augmented contact, triangle queries, adjacency, and self-contact settings;
- implicit thermal behavior and coupled degree-of-freedom layout;
- parameter binding, sampling, Pareto analysis, Gaussian-process support, and uncertainty summaries;
- scenario/document migration, result chunking/query, report determinism, extension trust/authentication, worker protocols, and release gates.

The Windows CI workflow also runs Python integrity audits for source structure, artifacts/migrations, external mesher contracts, process IPC, accessibility workflow coverage, and numerical reference fixtures before the .NET build and tests.

## Claim boundary

These checks establish build integrity and regression coverage for the repository. They do not certify physical accuracy. Credible predictive use still requires mesh/time-step convergence, calibrated material data, comparison with experiments or trusted reference solvers, and review appropriate to the engineering risk.

## Historical notes

Versioned files under `docs/architecture`, `docs/verification`, and `docs/finalization` record earlier design and pre-build milestones. Statements in those documents such as “SDK unavailable” or “runtime verification pending” describe their dated milestone and are superseded by this file for the current release candidate.
