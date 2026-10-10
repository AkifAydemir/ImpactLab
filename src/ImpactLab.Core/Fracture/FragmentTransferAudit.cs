using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public sealed record FragmentTransferAudit(
    int FragmentCount,
    double SourceMassKg,
    double ProjectedMassKg,
    double MassResidualKg,
    Vec3 SourceLinearMomentumKgMPerSec,
    Vec3 ProjectedLinearMomentumKgMPerSec,
    double LinearMomentumResidualKgMPerSec,
    Vec3 SourceIntrinsicAngularMomentumKgM2PerSec,
    Vec3 ProjectedIntrinsicAngularMomentumKgM2PerSec,
    double IntrinsicAngularMomentumResidualKgM2PerSec,
    double SourceKineticEnergyJ,
    double ProjectedKineticEnergyJ,
    double KineticEnergyResidualJ
)
{
    public bool IsConservative(double absoluteTolerance = 1e-9, double relativeTolerance = 1e-9)
    {
        if (absoluteTolerance < 0 || relativeTolerance < 0)
            throw new ArgumentOutOfRangeException(
                nameof(absoluteTolerance),
                "Conservation tolerances cannot be negative."
            );
        return WithinTolerance(MassResidualKg, SourceMassKg, absoluteTolerance, relativeTolerance)
            && WithinTolerance(
                LinearMomentumResidualKgMPerSec,
                SourceLinearMomentumKgMPerSec.Length,
                absoluteTolerance,
                relativeTolerance
            )
            && WithinTolerance(
                IntrinsicAngularMomentumResidualKgM2PerSec,
                SourceIntrinsicAngularMomentumKgM2PerSec.Length,
                absoluteTolerance,
                relativeTolerance
            )
            && WithinTolerance(
                KineticEnergyResidualJ,
                SourceKineticEnergyJ,
                absoluteTolerance,
                relativeTolerance
            );
    }

    private static bool WithinTolerance(
        double residual,
        double reference,
        double absoluteTolerance,
        double relativeTolerance
    ) => Math.Abs(residual) <= absoluteTolerance + relativeTolerance * Math.Abs(reference);
}
