namespace ImpactLab.Core.Sparse;

public sealed record LinearSolveReport(
    bool Converged,
    int Iterations,
    double ResidualNorm,
    string SolverId
);
