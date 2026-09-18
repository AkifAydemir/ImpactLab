namespace ImpactLab.Core.Experiments.Optimization;

public interface IOptimizer
{
    string Id { get; }
    Task<IReadOnlyList<OptimizationEvaluation>> OptimizeAsync(
        IReadOnlyList<OptimizationVariable> variables,
        Func<
            IReadOnlyDictionary<string, double>,
            CancellationToken,
            Task<(double Objective, bool Feasible, string? Artifact)>
        > evaluate,
        int maxEvaluations,
        CancellationToken ct = default
    );
}
