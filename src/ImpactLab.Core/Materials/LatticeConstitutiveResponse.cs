namespace ImpactLab.Core.Materials;

public readonly record struct LatticeConstitutiveResponse(
    double StressPa,
    double TangentModulusPa,
    double ElasticEnergyDensityJPerM3,
    double PlasticDissipationIncrementJPerM3
);
