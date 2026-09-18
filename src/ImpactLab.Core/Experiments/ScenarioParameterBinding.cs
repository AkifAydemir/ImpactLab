namespace ImpactLab.Core.Experiments;

public sealed record ScenarioParameterBinding(
    string Path,
    double Value,
    string? Unit = null,
    string? Label = null
);
