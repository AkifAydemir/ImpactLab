namespace ImpactLab.Core.Thermal;

public sealed record ThermalScenarioPackage(
    ThermalPropertyTable Properties,
    IReadOnlyList<ThermalBoundaryCondition> Boundaries,
    IReadOnlyList<ThermalSource> Sources
);
