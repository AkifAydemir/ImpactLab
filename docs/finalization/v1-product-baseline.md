# ImpactLab v1 source product baseline freeze

ImpactLab is frozen at the source level as an **extensible engineering simulation workbench with transparent numerical assumptions and representative workflows**. It is not presented as an ANSYS/Abaqus/LS-DYNA clone and does not claim predictive or certified FEA fidelity.

## Product-flow gap analysis

The Vol24 source already contains the major engineering capability surfaces: parametric/imported geometry, material catalogs and constitutive laws, loads and boundary definitions, contact, meshing, multiple simulation backends, run diagnostics, field/result queries, probes, charts, reports, persistence, experiments, worker/extension isolation and WPF workspaces. The final source pass therefore avoids new solvers, material families, CAD kernels, meshers, GPU/HPC paths or numerical scope expansion.

The bounded product-facing gaps were shell integration issues rather than missing numerical subsystems:

1. New/open scenario and built-in sample entry points existed in services/preset code but were not visible from the shell.
2. Backend selection existed as a view model but was not exposed in the primary workflow, and the product did not visibly state that runtime verification is pending.
3. The shell had a simulation runner and result/post-processing view models, but there was no direct scenario Solve -> Results transition.
4. The Simulation workspace rendered only a heading even though `MainViewModel` already exposed executable quick-impact inputs, status, timeline and result metrics.

The freeze pass closes only those bounded shell connections. It intentionally does **not** attempt a pre-build rewrite that merges every Geometry/Materials/Loads/Boundaries authoring view model into one live scenario editor. That integration remains a runtime/build-candidate follow-up because the current environment cannot compile WPF or execute the application.

## Visible user workflow

**NEW / OPEN ANALYSIS**

- `New Analysis` resets the workspace/scenario session.
- `Open Scenario...` opens an existing canonical scenario envelope.
- The shell exposes the two runtime presets already implemented by `ScenarioPresetLibrary`: `Plate impact study` and `Angle-profile comparison`.
- The sparse finite-strain and thermo-mechanical JSON files under `samples/v12` remain schema/reference fixtures, not runnable or verified examples.

**SETUP**

- Geometry, Import, Materials, Loads and Boundaries remain visible workspaces.
- The quick-impact sandbox exposes its existing target geometry/material, mesh cell size, impactor geometry/material, speed, duration and time-step inputs.
- A large pre-build rewrite that commits every independent authoring view model directly into the active scenario is intentionally deferred.

**SOLVE**

- The shell exposes the registered backend list and defaults to `explicit-lattice-v1` for the first smoke path.
- `Solve` executes the currently loaded scenario through the existing `SimulationRunnerService` and selected backend.
- Run status and failures are visible through an accessibility live region.
- Every backend is labeled **Experimental - runtime verification pending**. Portable source/reference contracts are explicitly not numerical validation.
- The Simulation workspace also exposes the pre-existing quick-impact sandbox `RunCommand`, timeline controls and result metrics.

**RESULTS**

- A successful shell-level scenario run loads `ResultExplorerViewModel` and `PostProcessingViewModel`, then transitions naturally to Results.
- Result quantity/frame selection, tabular node results and the existing large-result streaming surface are visible.
- Post-processing probes, Charts, Runs and Reports remain separate discoverable workspaces.

## Source-freeze limitations

- Real .NET 8 restore/build/analyzers/xUnit execution has not run in the current environment.
- Windows WPF launch, XAML compilation, keyboard/UI Automation, screen readers and end-to-end interaction have not run.
- Compiled numerical benchmarks, solver convergence/calibration and predictive validation have not run.
- The shell-level scenario workflow now exists in source, but the independent setup editors still require a compiled product pass before any deeper editor-to-scenario synchronization changes should be made.
- No backend is labeled verified in the product source.

## Required next target

External/local .NET 8 build-candidate verification -> restore/build/analyzers/xUnit -> WPF launch -> representative Setup/Solve/Results end-to-end workflow -> compiled numerical benchmarks -> worker/persistence/report/security smoke -> repair actual defects -> recanonicalize stabilized repository.
