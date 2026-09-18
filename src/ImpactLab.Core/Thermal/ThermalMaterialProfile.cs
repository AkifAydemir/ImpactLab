namespace ImpactLab.Core.Thermal;

public sealed record ThermalMaterialProfile(
    string MaterialId,
    TemperatureDependentProperty Conductivity,
    TemperatureDependentProperty SpecificHeat,
    TemperatureDependentProperty ExpansionCoefficient,
    double InelasticHeatFraction = 0.9
);
