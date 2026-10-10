# ImpactLab physics-engine architecture

ImpactLab is built as a multi-model impact engine, not as one universal discretization. A
simulation must use the least expensive model that still resolves the quantities needed for
the stated decision. Adding more particles or smaller elements is not automatically more
physical when the material law, boundary conditions, or validation evidence are inadequate.

## Model hierarchy

```text
Scenario and evidence
        |
        v
Scale / regime classifier
        |
        +--> resolved solid or lattice dynamics
        +--> continuum material and damage response
        +--> homogenized granular or porous response
        +--> thermal / equation-of-state response
        +--> reduced-order far-field response
        |
        v
Coupling and conservation ledger
        |
        v
Results + uncertainty + validity statement
```

Microscale entities are resolved only when their individual motion drives the requested
answer. For example, a granular region can normally use a calibrated bulk constitutive law;
selected ejecta or fragments can be converted into discrete bodies near a failure boundary.
This hybrid approach avoids making every grain a particle while retaining local detail where
it changes the result.

## Runtime model contract

`PhysicsModelRegistry` is the common registration and discovery point. Each model declares:

- a stable, versioned identifier;
- its physical category and current validation level;
- explicit dependencies on other models;
- the typed solver contract created by its factory.

`ScenarioPhysicsConfiguration` records the selected model identifiers in the scenario. Before
execution, the registry resolves dependencies in topological order and rejects unknown models
or dependency cycles. This makes the project reproducible and lets trusted extensions add
models without adding scenario-specific branches to the core solver.

The integrated selection points now include lattice damage, lattice constitutive response,
rigid contact and deformable contact. The same pattern is intended for failure/fragmentation,
thermal coupling, equations of state, granular media, fluids and cross-domain coupling.

## Fidelity rules

1. Every result must identify the active model versions.
2. Conservation channels must be explicit; an unimplemented channel is not reported as zero
   physical effect.
3. A model's validation level is evidence metadata, not a marketing label.
4. Model switching must conserve mass, momentum and energy within declared tolerances.
5. Local refinement is driven by error indicators or requested outputs, not visual detail.
6. Reduced-order and empirical models must publish their applicability range and source.
7. Safety-critical claims require convergence, calibration and independent experimental
   correlation for the intended regime.

## Implementation sequence

1. Complete the energy ledger: structural/global damping, external work, constraint work,
   plastic work and fracture energy.
2. Move constitutive, contact and failure choices behind typed registry contracts.
3. Add adaptive model regions and conservative state transfer between resolved and
   homogenized representations.
4. Add fragmentation with fragment generation, secondary contact and mass/momentum checks.
5. Add thermo-mechanical coupling, phase-change accounting and high-pressure material hooks.
6. Add calibrated granular/porous models and terrain-scale reduced-order coupling.
7. Build verification suites, convergence automation and experiment-correlation packages for
   each supported physical regime.

This architecture allows future high-energy ground-response or cratering research to be added
as general, source-traceable physics modules rather than a collection of hard-coded scenario
presets.

The first fragmentation foundation is implemented: stored lattice frames contain connected
clusters derived from intact links, with cluster mass, center of mass, linear momentum,
inertia tensor, angular momentum, angular velocity, rotational kinetic energy, bounds and
support attachment. A configurable representation policy classifies each cluster as resolved
deformable, reduced-order rigid or statistical. Distinct clusters retain their deformable nodal
motion and receive same-part secondary contact through the configured deformable-contact
model; secondary contacts and representation counts are recorded in frames and telemetry.
The state-transfer layer projects non-resolved clusters into coarse fragment states without
discarding mass, center of mass, linear momentum, angular momentum or kinetic energy. Kinetic
energy that cannot be represented as rigid translation plus rotation is retained explicitly as
an unresolved internal channel instead of silently disappearing. Independent time integration
is available for projected bodies, including gravity, accumulated force, off-center force,
torque, full-tensor angular acceleration and orientation updates. Stable fragment identities
prevent the same coarse body from being spawned more than once. Removing converted nodes from
the resolved lattice is available behind the experimental coarse-fragment integration setting:
converted nodes are masked from internal force, gravity, contact, kinetic-energy and rendering
paths while the independent body advances, preventing duplicate mass. Coarse-to-resolved and
coarse-to-coarse contact use the fragment's boundary-node samples, equal-and-opposite penalty
forces and history-dependent tangential springs with a static-to-dynamic Coulomb transition.
Normal and tangential contact-potential energy, sliding/damping dissipation and off-center torque
are accounted separately. Inactive contact histories are pruned when separation occurs. The
contact critical time step and force-balance residual are exported in telemetry. With the
setting disabled, coarse states remain conservative projections only.

Fragment inertia inversion is scale-relative and condition-number limited. Singular and badly
conditioned tensors stop the calculation with an explicit diagnostic instead of silently
producing zero angular acceleration. The current coarse contact remains an engineering
approximation: samples are spherical, rolling/torsional resistance is not modeled, broad-phase
acceleration is not yet implemented, and physical contact parameters still require calibration.

Fragment lineage is tracked from the intact topology through repeated resolved splits. A child
that keeps the parent's minimum-node identity continues the existing record; newly created
siblings record their parent, generation, birth step, birth time and original node membership.
Lineage is stored in simulation frames, while births and maximum generation are included in
telemetry and CSV output. Fragments that are already disconnected in the initial condition are
reported as roots because no earlier connected state exists from which a parent can be inferred.
