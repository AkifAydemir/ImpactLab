namespace ImpactLab.Core.Sparse.Iterative;

public sealed record IterativeSolveMetrics(
    bool Converged,
    int Iterations,
    double InitialResidual,
    double FinalResidual,
    TimeSpan Elapsed,
    IReadOnlyList<double> ResidualHistory
);
