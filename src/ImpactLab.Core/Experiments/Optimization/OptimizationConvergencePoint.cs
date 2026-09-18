namespace ImpactLab.Core.Experiments.Optimization;

public sealed record OptimizationConvergencePoint(
    int Evaluation,
    double BestValue,
    double Improvement,
    double FeasibleFraction
);
