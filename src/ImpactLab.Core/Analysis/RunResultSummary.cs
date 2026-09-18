namespace ImpactLab.Core.Analysis;

public sealed record RunResultSummary(
    double EndTimeSeconds,
    double PeakKineticEnergyJ,
    double PeakElasticEnergyJ,
    double MaxDisplacementMeters,
    int BrokenSpringCount,
    double PeakContactForceN,
    double PeakTangentialForceN,
    double FrictionEnergyJ,
    IReadOnlyList<PartResultSummary> Parts
);
