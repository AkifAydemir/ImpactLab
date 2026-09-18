namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearSolveFailure(
    double TimeSeconds,
    double AttemptedTimeStep,
    int CutbackLevel,
    int Iterations,
    double LastResidual,
    string Reason
);
