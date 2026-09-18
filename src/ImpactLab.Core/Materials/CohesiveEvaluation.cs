namespace ImpactLab.Core.Materials;

public readonly record struct CohesiveEvaluation(
    double NormalTractionPa,
    double ShearTractionPa,
    double Damage,
    double DissipatedEnergyDensityJPerM2,
    bool Failed
);
