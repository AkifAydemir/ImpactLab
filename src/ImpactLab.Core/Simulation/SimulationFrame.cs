using ImpactLab.Core.Contact;
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
    double FrictionEnergyJ
);
