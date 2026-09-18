namespace ImpactLab.Core.Materials;

public readonly record struct ConstitutiveEvaluation(
    double StressPa,
    double TangentModulusPa,
    double DissipatedEnergyDensityJPerM3,
    double DamageDrivingValue,
    bool Failed
);
