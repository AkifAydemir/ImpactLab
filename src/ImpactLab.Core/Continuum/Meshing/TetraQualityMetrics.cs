namespace ImpactLab.Core.Continuum.Meshing;

public readonly record struct TetraQualityMetrics(
    double Volume,
    double MeanRatio,
    double RadiusRatio,
    double MinDihedralDegrees,
    double MaxDihedralDegrees,
    bool Inverted
)
{
    public double EdgeRatio => MeanRatio <= 0 ? double.PositiveInfinity : 1.0 / MeanRatio;
}
