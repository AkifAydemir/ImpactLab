namespace ImpactLab.Core.Continuum.Dynamics;

public sealed record ContinuumDynamicSettings
{
    public bool Enabled { get; init; } = false;
    public TimeIntegratorKind Integrator { get; init; } = TimeIntegratorKind.CentralDifference;
    public MassMatrixMode MassMatrix { get; init; } = MassMatrixMode.Lumped;
    public double TimeStepSeconds { get; init; } = 1e-5;
    public double DurationSeconds { get; init; } = 0.01;
    public int OutputStride { get; init; } = 5;
    public int MaxSteps { get; init; } = 2_000_000;
    public ContinuumDampingSettings Damping { get; init; } = new();
    public bool EnablePlasticity { get; init; } = true;
    public bool EnableDynamicContact { get; init; } = true;
    public int StepCount =>
        Math.Min(MaxSteps, (int)Math.Ceiling(DurationSeconds / TimeStepSeconds));

    public void Validate()
    {
        if (TimeStepSeconds <= 0 || DurationSeconds <= 0 || OutputStride < 1 || MaxSteps < 1)
            throw new InvalidOperationException("Invalid transient continuum settings.");
        Damping.Validate();
    }
}
