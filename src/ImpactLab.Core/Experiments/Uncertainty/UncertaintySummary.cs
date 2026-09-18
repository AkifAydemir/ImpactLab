namespace ImpactLab.Core.Experiments.Uncertainty;

public sealed record UncertaintySummary(
    string Metric,
    int SampleCount,
    double Mean,
    double StandardDeviation,
    double P05,
    double P50,
    double P95,
    double Minimum,
    double Maximum
);
