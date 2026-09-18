namespace ImpactLab.Core.Thermal;

public sealed record ThermalSource(IReadOnlyList<int> NodeIds, double PowerWatts);
