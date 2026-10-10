using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public sealed record FragmentCoarseState(
    string Id,
    string PartId,
    FragmentRepresentationKind Representation,
    IReadOnlyList<int> SourceNodeIds,
    IReadOnlyList<FragmentContactSample> ContactSamples,
    double MassKg,
    Vec3 CenterOfMass,
    Vec3 LinearVelocity,
    Matrix3 InertiaTensorKgM2,
    double InertiaConditionNumber,
    Vec3 AngularMomentumKgM2PerSec,
    Vec3 AngularVelocityRadPerSec,
    QuaternionD Orientation,
    BoundingBox3 Bounds,
    double TranslationalKineticEnergyJ,
    double RotationalKineticEnergyJ,
    double UnresolvedInternalKineticEnergyJ
)
{
    public Vec3 LinearMomentumKgMPerSec => LinearVelocity * MassKg;

    public double AccountedKineticEnergyJ =>
        TranslationalKineticEnergyJ + RotationalKineticEnergyJ + UnresolvedInternalKineticEnergyJ;
}
