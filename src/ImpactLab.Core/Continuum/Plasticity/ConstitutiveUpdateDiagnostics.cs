namespace ImpactLab.Core.Continuum.Plasticity;

public readonly record struct ConstitutiveUpdateDiagnostics(
    bool Yielded,
    int LocalIterations,
    double YieldFunction,
    double PlasticMultiplier,
    double EquivalentPlasticIncrement,
    double DamageIncrement
);
