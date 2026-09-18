namespace ImpactLab.Core.Continuum.Plasticity;

public sealed record PlasticityExecutionSettings(
    HardeningModelKind Hardening = HardeningModelKind.Isotropic,
    ObjectiveStressRateKind ObjectiveRate = ObjectiveStressRateKind.None,
    bool EnableDamageCoupling = false,
    double MaximumPlasticIncrement = 0.02
);
