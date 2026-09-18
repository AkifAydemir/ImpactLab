namespace ImpactLab.Core.Experiments.Sensitivity;

public sealed record SensitivityIndex(
    string Parameter,
    double FirstOrder,
    double TotalOrder,
    double ConfidenceLow,
    double ConfidenceHigh
);
