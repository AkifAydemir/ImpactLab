namespace ImpactLab.Core.Thermal;

public sealed record RadiationBoundary(
    IReadOnlyList<int> NodeIds,
    double Emissivity,
    double AmbientTemperatureKelvin,
    double AreaPerNodeM2
);
