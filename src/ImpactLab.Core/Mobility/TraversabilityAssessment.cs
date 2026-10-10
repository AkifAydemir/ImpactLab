namespace ImpactLab.Core.Mobility;

public sealed record TraversabilityAssessment(
    TraversabilityStatus Status,
    double CostMultiplier,
    double GroundPressurePa,
    double BearingUtilization,
    double SlopeUtilization,
    double FrictionUtilization,
    double ObstacleUtilization,
    double WaterUtilization,
    double StructuralUtilization,
    double Confidence,
    IReadOnlyList<string> Reasons
);
