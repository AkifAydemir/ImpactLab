namespace ImpactLab.Core.Sparse;

public sealed record SparseSolveResult(
    double[] Solution,
    bool Converged,
    int Iterations,
    double FinalResidual,
    TimeSpan Elapsed
);
