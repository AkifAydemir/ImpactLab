using ImpactLab.Core.Continuum.Dynamics;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearTransientSettings
{
    public double TimeStepSeconds { get; init; } = 1e-4;
    public double DurationSeconds { get; init; } = 0.02;
    public double NewmarkBeta { get; init; } = 0.25;
    public double NewmarkGamma { get; init; } = 0.5;
    public ContinuumDampingSettings Damping { get; init; } = new();
    public NonlinearConvergenceSettings Convergence { get; init; } = new();
    public LineSearchSettings LineSearch { get; init; } = new();
    public int OutputStride { get; init; } = 1;
    public int MaxCutbacks { get; init; } = 6;
    public double MinimumTimeStepSeconds { get; init; } = 1e-8;

    public void Validate()
    {
        if (
            TimeStepSeconds <= 0
            || DurationSeconds <= 0
            || NewmarkBeta <= 0
            || NewmarkGamma <= 0
            || OutputStride < 1
            || MaxCutbacks < 0
            || MinimumTimeStepSeconds <= 0
        )
            throw new InvalidOperationException("Invalid nonlinear transient settings.");
        Damping.Validate();
        Convergence.Validate();
        LineSearch.Validate();
    }
}
