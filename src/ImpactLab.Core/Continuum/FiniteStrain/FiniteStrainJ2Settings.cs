namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainJ2Settings(
    double YieldStressPa = 250e6,
    double IsotropicHardeningPa = 1e9,
    double KinematicHardeningPa = 0,
    double DamageOnsetPlasticStrain = 0.25,
    double FailurePlasticStrain = 0.8
);
