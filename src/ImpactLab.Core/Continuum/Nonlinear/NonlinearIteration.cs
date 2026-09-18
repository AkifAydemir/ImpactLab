namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearIteration(
    int LoadStep,
    int Iteration,
    double LoadFactor,
    double ResidualNorm,
    bool Converged
);
