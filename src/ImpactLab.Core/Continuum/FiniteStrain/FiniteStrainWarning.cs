namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainWarning(
    int ElementId,
    double TimeSeconds,
    string Code,
    string Message
);
