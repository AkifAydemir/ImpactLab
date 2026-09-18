namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearContinuumSettings(
    int LoadSteps = 10,
    int MaxNewtonIterations = 12,
    double RelativeTolerance = 1e-5,
    double MinimumStiffnessScale = 0.02
);
