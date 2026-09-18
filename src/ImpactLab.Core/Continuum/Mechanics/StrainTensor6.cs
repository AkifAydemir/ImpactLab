namespace ImpactLab.Core.Continuum.Mechanics;

public readonly record struct StrainTensor6(
    double XX,
    double YY,
    double ZZ,
    double XY,
    double YZ,
    double ZX
);
