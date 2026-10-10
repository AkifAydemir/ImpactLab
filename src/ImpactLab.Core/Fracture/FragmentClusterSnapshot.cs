using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public sealed record FragmentClusterSnapshot(
    string Id,
    string PartId,
    IReadOnlyList<int> NodeIds,
    double MassKg,
    Vec3 CenterOfMass,
    Vec3 LinearVelocity,
    BoundingBox3 Bounds,
    bool ContainsConstrainedNode,
    IReadOnlyList<FragmentContactSample> ContactSamples,
    Matrix3 InertiaTensorKgM2,
    double InertiaConditionNumber,
    Vec3 AngularMomentumKgM2PerSec,
    Vec3 AngularVelocityRadPerSec,
    double RotationalKineticEnergyJ,
    double TotalNodalKineticEnergyJ,
    double TranslationalKineticEnergyJ,
    double UnresolvedInternalKineticEnergyJ
)
{
    public Vec3 LinearMomentumKgMPerSec => LinearVelocity * MassKg;

    public double ProjectedKineticEnergyJ =>
        TranslationalKineticEnergyJ + RotationalKineticEnergyJ + UnresolvedInternalKineticEnergyJ;
}
