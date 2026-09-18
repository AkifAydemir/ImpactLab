namespace ImpactLab.Core.Materials;

public readonly record struct MaterialTestSample(
    double Strain,
    double StressPa,
    double? StrainRatePerSecond = null,
    double? TemperatureK = null,
    double Weight = 1.0
);
