namespace ImpactLab.Core.Materials;

public sealed record MaterialCalibrationMetrics(
    double RmsePa,
    double MaePa,
    double RSquared,
    int SampleCount,
    string ModelName
);
