namespace ImpactLab.Core.Sparse.Native;

public sealed record NativeSolveResponse(
    bool Success,
    double[] Solution,
    int Iterations,
    double ResidualNorm,
    string? Error
);
