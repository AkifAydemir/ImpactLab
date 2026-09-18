namespace ImpactLab.Core.Sparse;

public sealed record LinearSolveDiagnostics(
    string SolverId,
    bool Converged,
    int Iterations,
    double Residual,
    double RelativeResidual,
    TimeSpan Elapsed,
    int MatrixRows,
    int NonZeros
);
