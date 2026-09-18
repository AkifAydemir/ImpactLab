using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Constraints;

public readonly record struct ConstraintReactionSample(
    string ConstraintId,
    double TimeSeconds,
    Vec3 ForceN,
    double MagnitudeN,
    int CorrectedNodeCount
);
