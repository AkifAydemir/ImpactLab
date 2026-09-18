namespace ImpactLab.Core.Continuum.Nonlinear;

public readonly record struct NonlinearIterationMetrics(
    int Iteration,
    double ResidualNorm,
    double RelativeResidual,
    double IncrementNorm,
    double RelativeIncrement,
    double EnergyNorm,
    double LineSearchScale,
    bool Converged
);
