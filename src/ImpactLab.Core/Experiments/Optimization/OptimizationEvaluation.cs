namespace ImpactLab.Core.Experiments.Optimization;

public sealed record OptimizationEvaluation(
    int Iteration,
    IReadOnlyDictionary<string, double> Parameters,
    double Objective,
    bool Feasible,
    string? ArtifactPath = null
);
