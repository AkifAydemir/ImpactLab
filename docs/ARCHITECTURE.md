# Architecture

ImpactLab separates the desktop product, numerical domain, and untrusted/parallel execution boundaries so the core can be tested without launching WPF.

## Main layers

### Desktop application

`ImpactLab.App` is the composition root for the WPF workbench. View models expose the engineering flow—scenario setup, backend selection, solve status, replay, field exploration, probes, experiments, charts, archives, reports, extensions, and diagnostics. `SimulationRunnerService` compiles the selected scenario, invokes a backend, and adapts its immutable result for the UI.

### Numerical and workflow core

`ImpactLab.Core` owns the domain model and algorithms. Its major subsystems are:

- geometry primitives, imported meshes, repair, and topology inspection;
- adaptive lattice and structured tetrahedral meshing;
- explicit lattice, linear continuum, transient, nonlinear, and coupled analysis paths;
- material profiles, constitutive laws, plasticity, contact, and rigid bodies;
- CSR sparse storage, preconditioners, and iterative/direct solver adapters;
- result domains, field queries, chunked storage, probes, visualization documents, and reporting;
- parameter sweeps, optimization, uncertainty, calibration, and verification;
- versioned scenario/document migration and deterministic persistence.

Backends return `BackendRunOutput`, which keeps a compatibility `SimulationResult`, a typed result domain, and an optional native result. This lets the shell share common reporting and telemetry while continuum consumers retain richer fields.

### Process isolation

`ImpactLab.Worker` executes experiment cases through a versioned request/response protocol and writes hashed result artifacts. Heartbeats and bounded concurrency make long parameter studies observable.

`ImpactLab.ExtensionHost` is the out-of-process boundary for extensions that are not explicitly trusted for in-process loading. Compatibility, publisher trust, allow-list, and IPC authentication policies live in the core rather than being implicit UI behavior.

## Representative solve flow

1. A scenario is loaded or created and validated.
2. `ScenarioCompiler` resolves geometry, materials, selections, loads, constraints, probes, rigid bodies, and contact rules.
3. `SimulationBackendRegistry` selects the requested backend.
4. The backend produces immutable frames, telemetry, diagnostics, and/or a native continuum result.
5. Adapters feed result exploration, probes, energy analysis, charts, archives, and deterministic reports.
6. Verification code evaluates analytical benchmarks and reference signals independently of the UI.

## Design boundaries

- The WPF layer depends on the core; the core does not depend on WPF.
- Numerical results are passed as immutable run outputs rather than mutated by views.
- Scenario and evidence formats are versioned and migrated explicitly.
- Build/test evidence and portable source-integrity evidence are separate claims.
- Experimental capability is not described as validated engineering accuracy.
