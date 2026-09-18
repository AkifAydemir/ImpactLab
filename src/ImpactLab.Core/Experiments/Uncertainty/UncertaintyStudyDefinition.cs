namespace ImpactLab.Core.Experiments.Uncertainty;

public sealed record UncertaintyStudyDefinition(
    string Id,
    int SampleCount,
    int Seed,
    IReadOnlyList<ParameterDistribution> Parameters,
    string ObjectiveMetric
);
