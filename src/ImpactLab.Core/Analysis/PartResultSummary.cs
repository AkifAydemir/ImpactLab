namespace ImpactLab.Core.Analysis;

public sealed record PartResultSummary(
    string PartId,
    int NodeCount,
    int SpringCount,
    int BrokenSpringCount,
    double BrokenSpringFraction,
    double MaxDisplacementMeters,
    double MaxNodeDamage,
    double ResidualKineticEnergyJ
);
