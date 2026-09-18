namespace ImpactLab.Core.Thermal;

public sealed record ThermalBoundaryCondition(IReadOnlyList<int> NodeIds, double TemperatureKelvin);
