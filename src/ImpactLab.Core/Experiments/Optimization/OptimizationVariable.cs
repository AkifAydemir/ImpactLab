namespace ImpactLab.Core.Experiments.Optimization;

public sealed record OptimizationVariable(
    string Id,
    double Initial,
    double Minimum,
    double Maximum,
    double Scale = 1.0
);
