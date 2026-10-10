using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Simulation;

public sealed record SimulationFrame(
    double TimeSeconds,
    Vec3[] Position,
    Vec3[] Velocity,
    double[] NodeDamage,
    double KineticEnergyJ,
    double RotationalKineticEnergyJ,
    double ElasticEnergyJ,
    IReadOnlyList<RigidBodyFrame> RigidBodies,
    IReadOnlyList<ContactPairMetrics> ContactPairs,
    IReadOnlyList<DeformableContactMetrics> DeformableContacts,
    int ContactCount,
    double MaxContactPenetrationMeters,
    double TotalNormalContactForceN,
    double TotalTangentialContactForceN,
    double FrictionEnergyJ,
    double ContactPotentialEnergyJ = 0.0,
    double ContactDampingEnergyJ = 0.0,
    double PlasticDissipationEnergyJ = 0.0,
    double FractureDissipationEnergyJ = 0.0,
    double ThermalEnergyJ = 0.0,
    double ExternalWorkJ = 0.0,
    double ConstraintWorkJ = 0.0,
    double StructuralDampingEnergyJ = 0.0,
    double GlobalDampingEnergyJ = 0.0,
    IReadOnlyList<FragmentClusterSnapshot>? FragmentClusters = null,
    int SecondaryFragmentContactCount = 0,
    IReadOnlyList<FragmentRepresentationDecision>? FragmentRepresentations = null,
    IReadOnlyList<FragmentCoarseState>? FragmentCoarseStates = null,
    FragmentTransferAudit? FragmentTransferAudit = null,
    bool[]? NodeActive = null,
    IReadOnlyList<CoarseFragmentContactMetrics>? CoarseFragmentContacts = null,
    IReadOnlyList<FragmentLineageRecord>? FragmentLineage = null
);
