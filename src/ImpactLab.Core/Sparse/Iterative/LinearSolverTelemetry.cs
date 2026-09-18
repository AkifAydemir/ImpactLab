namespace ImpactLab.Core.Sparse.Iterative;

public sealed record LinearSolverTelemetry(
    string SolverId,
    string PreconditionerId,
    int Dofs,
    int NonZeros,
    int Iterations,
    double Residual,
    TimeSpan Elapsed,
    bool Native
);
