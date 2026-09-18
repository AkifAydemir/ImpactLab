namespace ImpactLab.Core.Experiments.Sensitivity;

public sealed record SensitivityConfidenceInterval(
    double Estimate,
    double Lower,
    double Upper,
    int BootstrapSamples
);
