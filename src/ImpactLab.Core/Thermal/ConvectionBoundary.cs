namespace ImpactLab.Core.Thermal;

public sealed record ConvectionBoundary(
    IReadOnlyList<int> NodeIds,
    double HeatTransferCoefficientWPerM2K,
    double AmbientTemperatureKelvin,
    double AreaPerNodeM2
);
