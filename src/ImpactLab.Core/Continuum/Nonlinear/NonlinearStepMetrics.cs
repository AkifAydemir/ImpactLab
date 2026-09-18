namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearStepMetrics(
    double TimeSeconds,
    double TimeStepSeconds,
    int Iterations,
    bool Converged,
    int CutbackLevel,
    IReadOnlyList<NonlinearIterationMetrics> IterationHistory
);
