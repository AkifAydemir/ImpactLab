using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Rigid;

namespace ImpactLab.Core.Scenarios;

public sealed record CompiledScenario(
    SimulationMesh Mesh,
    IReadOnlyList<IFixedConstraint> Constraints,
    IReadOnlyList<IKinematicConstraint> KinematicConstraints,
    IReadOnlyList<ILoadSource> Loads,
    IReadOnlyList<ProbeDefinition> Probes,
    IReadOnlyList<RigidBodyDefinition> RigidBodies,
    ContactInteractionTable ContactInteractions,
    DeformableContactTable DeformableInteractions
);
