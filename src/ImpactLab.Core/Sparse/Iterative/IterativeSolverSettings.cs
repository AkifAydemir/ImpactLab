namespace ImpactLab.Core.Sparse.Iterative;

public sealed record IterativeSolverSettings(
    int MaxIterations = 5000,
    double RelativeTolerance = 1e-8,
    double AbsoluteTolerance = 1e-12,
    int Restart = 64,
    bool UsePreconditioner = true
);
