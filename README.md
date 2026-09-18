<p align="center">
  <img src="docs/assets/impactlab-mark.svg" width="112" alt="ImpactLab mark">
</p>

# ImpactLab

[Türkçe](README.tr.md)

**A local Windows engineering workbench for impact simulation, continuum mechanics, thermal coupling, experiments, and reproducible verification.**

ImpactLab turns an engineering scenario into a traceable workflow: define geometry and materials, apply loads and boundaries, select a numerical backend, solve, inspect fields and probes, compare experiments, and export a deterministic report package. The desktop shell is WPF; the numerical and workflow layers are split into reusable .NET libraries and isolated worker processes.

> Personal engineering project started during high school. It is published as an experimental workbench and learning platform—not as a certified FEA product or a replacement for validated commercial solvers.

![Completed Quick Impact Sandbox run with 3D result and telemetry in the Windows Release build](docs/assets/impactlab-quick-impact-release-clean.jpg)

*Real Release-build capture: a completed Quick Impact Sandbox run with its 3D result and telemetry visible. The image is a software demonstration, not evidence of physical accuracy.*

## What makes it interesting

- Multiple analysis paths: explicit lattice impact, linear-static tetrahedral continuum, transient dynamics, nonlinear/finite-strain foundations, and thermo-mechanical coupling.
- Engineering mechanics beyond a UI mock-up: sparse CSR matrices, iterative solvers, tetrahedral assembly, J2 plasticity, Neo-Hookean material response, penalty/friction contact, and energy diagnostics.
- End-to-end product surface: scenario migration, mesh import/repair, parameter sweeps, uncertainty and optimization tools, result streaming, probes, charts, reports, and run archives.
- Process boundaries where they matter: experiment workers and untrusted extensions use versioned IPC contracts; extension trust and compatibility policies are explicit.
- Verification is part of the architecture: analytical benchmarks, reference-signal comparison, schema migration tests, deterministic report checks, integrity audits, and CI evidence artifacts.

## How the workbench is built

A scenario is the unit that connects the product surface to the numerical
code. The WPF app collects geometry, materials, loads, boundaries, and backend
settings; versioned scenario data and migrations keep saved inputs readable.
The selected backend runs through `ImpactLab.Core`, while the desktop presents
the resulting fields, telemetry, probes, charts, and report/export paths.

The implementation separates jobs with different failure and trust profiles.
The numerical and domain code lives in a reusable library; parameter studies
can run in an isolated worker; extensions have a separate host and explicit
compatibility/trust policy. Analytical/reference tests, schema fixtures, and
integrity audits check parts of that system independently. They establish
software behavior and regression evidence, not physical accuracy for an
unmeasured real-world impact.

## Verified status

The current Windows release candidate was rebuilt from source on .NET 8 with analyzers and warnings-as-errors enabled.

| Gate | Result |
| --- | --- |
| Release solution build | **Passed — 0 warnings, 0 errors** |
| xUnit suite | **Passed — 111/111** |
| Source formatting | **Passed — 1,171 files checked** |
| Portable integrity audits | **Passed — 6/6** |
| WPF startup smoke test | **Passed — process remained healthy for 8 seconds** |
| Interactive Quick Impact run | **Completed in the Release app — 3.000 ms simulated, 235 ms compute, 31 telemetry samples** |
| Built-in scenario UI flow | **Plate impact study: Load Sample → Solve → Results Explorer completed in the Release app** |
| Projects built together | Core, WPF App, Worker, ExtensionHost, Extractor, MeshAdapter sample, Tests |
| GitHub Actions | **Passed** — Windows integrity audits, formatting, Release build, and tests in the [published workflow run](https://github.com/AkifAydemir/ImpactLab/actions/runs/35299671087) |

See [validation details](docs/VALIDATION.md) for the exact commands and claim boundaries.

The workflow also runs on subsequent pushes; check [current runs](https://github.com/AkifAydemir/ImpactLab/actions) for their status. Current desktop package version: **1.0.0**.

## Architecture at a glance

```text
WPF workbench
  ├─ scenario authoring and schema migration
  ├─ geometry, materials, loads, boundaries
  ├─ solve orchestration and backend registry
  └─ results, probes, charts, reports, archives
                    │
                    ▼
               ImpactLab.Core
  ├─ lattice + tetrahedral continuum mechanics
  ├─ contact + constitutive laws + thermal coupling
  ├─ sparse solvers + numerical diagnostics
  └─ experiments + verification + persistence
          │                         │
          ▼                         ▼
 isolated experiment worker    isolated extension host
```

The fuller dependency and data-flow description is in [architecture](docs/ARCHITECTURE.md).

## Following a scenario through the system

The scenario begins as versioned input rather than UI-only state. Loading or
creating one passes geometry, materials, selections, loads, constraints,
probes, rigid bodies, and contact rules to `ScenarioCompiler`. That compiler
checks and resolves the selections before `SimulationBackendRegistry` chooses
the requested analysis path. This split lets the WPF shell manage interaction
without embedding numerical equations inside views or view models.

A backend returns an immutable `BackendRunOutput`: compatibility result data,
a typed result domain, and, where applicable, a native continuum result.
Adapters then make frames, fields, telemetry, diagnostics, probes, charts,
archives, and deterministic reports available to the relevant workspace. The
same result boundary supports both the quick lattice demonstration and richer
continuum consumers without pretending that every backend produces identical
fields. The built-in Plate impact study has been observed reaching Results
Explorer; that is evidence of the application flow, not a validated impact
prediction.

Longer or less trusted work is not hidden inside the WPF UI thread. Experiment
cases can run through a versioned worker protocol with heartbeats, bounded
concurrency, and hashed artifacts. Extensions that are not trusted for
in-process loading use a separate host and explicit compatibility/trust checks.
Versioned document migrations and report fixtures keep older inputs and
generated artifacts testable. The 111 tests and six integrity audits cover
many of these contracts, while physical validation still needs convergence,
material calibration, and comparison with independent measurements.

### Where to read the implementation

| File | What to inspect |
| --- | --- |
| [`src/ImpactLab.Core/Scenarios/ScenarioCompiler.cs`](src/ImpactLab.Core/Scenarios/ScenarioCompiler.cs) | Scenario validation and resolved solve inputs. |
| [`src/ImpactLab.Core/Backends/SimulationBackendRegistry.cs`](src/ImpactLab.Core/Backends/SimulationBackendRegistry.cs) | Backend selection boundary. |
| [`src/ImpactLab.Core/Backends/BackendRunOutput.cs`](src/ImpactLab.Core/Backends/BackendRunOutput.cs) | Shared immutable run-result contract. |
| [`src/ImpactLab.App/Services/SimulationRunnerService.cs`](src/ImpactLab.App/Services/SimulationRunnerService.cs) | UI-to-solver orchestration. |
| [`src/ImpactLab.Worker/WorkerHost.cs`](src/ImpactLab.Worker/WorkerHost.cs) | Isolated experiment execution. |
| [`src/ImpactLab.ExtensionHost/ExtensionHostRuntime.cs`](src/ImpactLab.ExtensionHost/ExtensionHostRuntime.cs) | Out-of-process extension runtime. |
| [`eng/verify.ps1`](eng/verify.ps1) | Repeatable Windows verification sequence. |

## Start with the verified interactive path

1. Open **Simulation → Quick Impact Sandbox** in the Windows Release app.
2. Set **Mesh cell size** to `20 mm` and **Time step** to `20 µs`; leave the other default inputs unchanged.
3. Select **Run Quick Impact**. The representative run captured above completed with a 3D scene and 31 telemetry samples.

For a second UI path, select the built-in **Plate impact study**, keep **Explicit Lattice Dynamics**, and choose **Solve**. The Release app completed this coarse preset and opened **Results Explorer** at frame `15`. This verifies the workflow transition and result-row generation, not meaningful impact response or physical accuracy. The stronger visual demonstration remains Quick Impact above.

## Quick start

Requirements: Windows 10/11 and a .NET 8 SDK.

```powershell
dotnet restore .\ImpactLab.sln
dotnet build .\ImpactLab.sln -c Release --no-restore
dotnet test .\ImpactLab.sln -c Release --no-build
dotnet run --project .\src\ImpactLab.App\ImpactLab.App.csproj -c Release
```

For the repository’s evidence-producing verification path:

```powershell
dotnet tool restore
dotnet tool run csharpier check .
.\eng\verify.ps1
```

For a short live demo, use the verified Quick Impact path above. The built-in scenario flow and its limits are recorded in [validation details](docs/VALIDATION.md).

## Repository map

| Path | Responsibility |
| --- | --- |
| `src/ImpactLab.Core` | Numerical kernels, domain models, backends, experiments, results, persistence, verification |
| `src/ImpactLab.App` | Windows/WPF engineering workbench |
| `src/ImpactLab.Worker` | Isolated parameter-study execution |
| `src/ImpactLab.ExtensionHost` | Out-of-process extension boundary |
| `tests/ImpactLab.Core.Tests` | Unit, numerical-contract, security, migration, and workflow tests |
| `eng` | Release verification and portable integrity audits |
| `samples` | Scenario, mesher-contract, accessibility, extension-policy, and report fixtures |
| `docs` | Current architecture/validation notes plus versioned design history |

Small, reviewable changes are welcome; see [CONTRIBUTING.md](CONTRIBUTING.md) for the local quality gates.

## Scope and limitations

ImpactLab demonstrates software architecture and numerical engineering practice. Passing tests show that the implemented contracts and included analytical/reference cases behave as expected; they do **not** establish predictive accuracy for arbitrary real-world structures. Production use would require convergence studies, experimental correlation, broader element/contact validation, material calibration, and independent review.

That boundary is deliberate: the project aims to make assumptions and evidence visible instead of hiding them behind impressive screenshots.

## License

Licensed under the [MIT License](LICENSE).
