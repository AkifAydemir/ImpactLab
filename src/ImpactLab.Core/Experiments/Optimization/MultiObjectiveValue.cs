namespace ImpactLab.Core.Experiments.Optimization;

public sealed record MultiObjectiveValue(
    IReadOnlyList<double> Objectives,
    IReadOnlyList<double> Constraints,
    string CaseId
);
