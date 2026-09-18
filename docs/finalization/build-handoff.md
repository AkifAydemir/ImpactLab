# ImpactLab v1 build-candidate handoff

This note is derived from the frozen canonical source/project configuration. It describes exact build/runtime entry points present in the repository; it does not claim they have succeeded in the current environment.

## Toolchain and solution

- Solution: `ImpactLab.sln`
- SDK pin: `global.json` -> .NET SDK `8.0.100`, `rollForward: latestFeature`, `allowPrerelease: false`.
- Shared build policy: `Directory.Build.props` -> nullable enabled, implicit usings enabled, warnings as errors, deterministic build, `AnalysisLevel=latest-recommended`.
- WPF startup project: `src/ImpactLab.App/ImpactLab.App.csproj`
  - `OutputType=WinExe`
  - `TargetFramework=net8.0-windows`
  - `UseWPF=true`
  - `EnableWindowsTargeting=true`
- Core library: `src/ImpactLab.Core/ImpactLab.Core.csproj` -> `net8.0`.
- Test project: `tests/ImpactLab.Core.Tests/ImpactLab.Core.Tests.csproj` -> xUnit + Microsoft.NET.Test.Sdk.
- Worker executable project: `src/ImpactLab.Worker/ImpactLab.Worker.csproj` -> `OutputType=Exe`, `net8.0`.
- Extension-host executable project: `src/ImpactLab.ExtensionHost/ImpactLab.ExtensionHost.csproj` -> `OutputType=Exe`, `net8.0`.
- External-mesher fixture executable project: `tools/ImpactLab.MeshAdapter.Sample/ImpactLab.MeshAdapter.Sample.csproj` -> `OutputType=Exe`, `net8.0`.
- Canonical extractor executable project: `tools/ImpactLab.Extractor/ImpactLab.Extractor.csproj` -> `OutputType=Exe`, `net8.0`.

The solution contains exactly these seven projects: `ImpactLab.Core`, `ImpactLab.App`, `ImpactLab.Core.Tests`, `ImpactLab.Worker`, `ImpactLab.MeshAdapter.Sample`, `ImpactLab.ExtensionHost`, `ImpactLab.Extractor`.

## Canonical restore/build/analyzer/test sequence

Preferred Windows entrypoint:

```powershell
./eng/verify.ps1
```

POSIX entrypoint after text-only canonical extraction:

```bash
bash eng/verify.sh
```

Both scripts execute the same toolchain sequence:

```text
dotnet restore ./ImpactLab.sln
dotnet build ./ImpactLab.sln -c Release --no-restore
dotnet test ./tests/ImpactLab.Core.Tests/ImpactLab.Core.Tests.csproj -c Release --no-build --results-directory ./artifacts/verification/test-results --logger "trx;LogFileName=ImpactLab.Core.Tests.trx"
```

Because `Directory.Build.props` enables `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-recommended`, the Release solution build is the canonical compiler/analyzer gate.

## WPF launch

After restore/build succeeds on Windows:

```powershell
dotnet run --project ./src/ImpactLab.App/ImpactLab.App.csproj -c Release
```

The startup object is the WPF `App` declared by `src/ImpactLab.App/App.xaml`, whose `StartupUri` is `MainWindow.xaml`.

## First meaningful end-to-end product smoke

1. Launch `ImpactLab.App`.
2. Confirm the shell visibly labels backends experimental/runtime-verification-pending.
3. Select built-in preset `Plate impact study` from `ScenarioPresetLibrary`.
4. Confirm backend `Explicit Lattice Dynamics` (`explicit-lattice-v1`) is selected by default.
5. Run `Solve`.
6. Confirm live status reaches completion without an unhandled exception.
7. Confirm the shell transitions to Results and `ResultExplorerViewModel` contains rows for the run.
8. Change result quantity/frame and inspect data refresh.
9. Open Post-processing and confirm probe/section surfaces do not throw when the run provides those outputs.
10. Repeat with built-in preset `Angle-profile comparison`.

The separate `Quick Impact Sandbox` in the Simulation workspace exercises the pre-existing `MainViewModel.RunCommand` explicit-lattice demonstration path and its frame/timeline/metrics surfaces. It is an additional UI smoke, not a substitute for the shell-level scenario flow.

## Compiled numerical verification entry points

Run the full xUnit suite first through the canonical verify script. Relevant compiled numerical/reference tests present in `tests/ImpactLab.Core.Tests` include:

- `VerificationBenchmarkRunnerTests`
- `VerificationGateEvaluatorTests`
- `PureRotationObjectivityBenchmarkTests`
- `NeoHookeanUniaxialBenchmarkTests`
- `NeoHookeanTests`
- `FiniteStrainMatrixTests`
- `AugmentedContactTests`
- `PenaltyFrictionContactLawTests`
- `ThermalBoundaryTests`
- `ImplicitThermalSolverTests`

A targeted follow-up, after the full suite compiles, may use:

```powershell
dotnet test ./tests/ImpactLab.Core.Tests/ImpactLab.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~VerificationBenchmarkRunnerTests|FullyQualifiedName~VerificationGateEvaluatorTests|FullyQualifiedName~PureRotationObjectivityBenchmarkTests|FullyQualifiedName~NeoHookeanUniaxialBenchmarkTests|FullyQualifiedName~NeoHookeanTests|FullyQualifiedName~FiniteStrainMatrixTests|FullyQualifiedName~AugmentedContactTests|FullyQualifiedName~PenaltyFrictionContactLawTests|FullyQualifiedName~ThermalBoundaryTests|FullyQualifiedName~ImplicitThermalSolverTests"
```

`eng/numerical_reference_integrity.py` remains portable source/reference-contract evidence only and must not be substituted for these compiled tests or solver validation.

## Separate process/executable smoke targets

Worker source requires its first argument to be `worker`:

```powershell
dotnet run --project ./src/ImpactLab.Worker/ImpactLab.Worker.csproj -c Release -- worker
```

`ImpactLab.Worker.Program` then serves its stdin/stdout protocol through `WorkerHost`.

Extension host is a stdin/stdout service with no command-line mode required by `Program.cs`:

```powershell
dotnet run --project ./src/ImpactLab.ExtensionHost/ImpactLab.ExtensionHost.csproj -c Release
```

The deterministic external-mesher fixture requires request and result paths:

```powershell
dotnet run --project ./tools/ImpactLab.MeshAdapter.Sample/ImpactLab.MeshAdapter.Sample.csproj -c Release -- ./samples/v15/external-mesher/mesher-request.json ./artifacts/verification/mesher-result.json
```

The sample adapter intentionally accepts exactly four CAD vertices and emits one tetrahedron; it is a wire-contract fixture, not a production tetrahedralizer.

Canonical extractor invocation after the solution builds:

```powershell
dotnet run --project ./tools/ImpactLab.Extractor/ImpactLab.Extractor.csproj -c Release -- ./ImpactLab_Vol24.docx ./artifacts/extracted-source <expected-aggregate-sha256>
```

Use the aggregate SHA-256 published by the delivered build-candidate manifest/canonical Section 6.3.

## Runtime resources, fixtures and samples

- Runnable in-source presets: `src/ImpactLab.Core/Scenarios/ScenarioPresetLibrary.cs`
  - `Plate impact study`
  - `Angle-profile comparison`
- Reference/schema fixtures: `samples/v12`, `samples/v15`, `samples/v16`.
- xUnit fixtures: `tests/ImpactLab.Core.Tests/Fixtures`.
- Report golden package: `samples/v15/report-golden`.
- External-mesher request/result fixture: `samples/v15/external-mesher`.
- Accessibility workflow contract fixture: `samples/v16/accessibility-workflow.json`.
- Extension-host policy fixture: `samples/v16/extension-host-policy.json`.

The `samples/v12/finite-strain-plate/scenario.json` and `samples/v12/thermo-mechanical-block/scenario.json` payloads are sparse schema/reference fixtures. They are not labeled as end-to-end runnable or numerically verified product examples.

## What remains NOT VERIFIED

- Actual .NET 8 restore/build/analyzer/xUnit pass.
- WPF XAML compilation and Windows launch.
- Representative New/Open -> Setup -> Solve -> Results workflow execution.
- Unified live commit wiring from every independent Geometry/Materials/Loads/Boundaries editor into the current shell scenario.
- Compiled numerical benchmark execution, solver convergence, calibration and predictive/certified fidelity.
- Large-result persistence/query/index/chunk-codec/prefetch runtime behavior and performance.
- Worker process orchestration, extension-host authenticated IPC/security behavior and OS-isolation strength.
- Persistence/migration/report package runtime smoke.
- Keyboard-only, UI Automation, high-contrast and screen-reader behavior.

Compiler/runtime/test defects found during the first external build are expected build-candidate feedback. Repair the extracted repository first, then synchronize corrected full files back into Section 7 and regenerate Section 6.3 before claiming a stabilized/release candidate.
