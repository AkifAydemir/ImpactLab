namespace ImpactLab.Core.Experiments.Optimization;

public sealed record OptimizationTrace(
    OptimizationEvaluation Best,
    IReadOnlyList<OptimizationEvaluation> Evaluations
);
