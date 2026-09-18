namespace ImpactLab.Core.Sparse;

public sealed record ScalableLinearSolverSettings(
    string SolverId = "bicgstab",
    string PreconditionerId = "jacobi",
    bool PreferNative = false,
    int MaxDegreeOfParallelism = 0,
    long NativeThresholdDofs = 250000
);
