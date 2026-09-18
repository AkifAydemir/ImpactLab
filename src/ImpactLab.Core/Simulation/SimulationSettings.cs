using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Simulation;

public sealed record SimulationSettings
{
    public double TimeStepSeconds { get; init; } = 0.000002;
    public double DurationSeconds { get; init; } = 0.001;
    public int SnapshotStride { get; init; } = 10;
    public int TelemetryStride { get; init; } = 10;
    public Vec3 Gravity { get; init; } = Vec3.Zero;
    public double GlobalVelocityDamping { get; init; } = 0.002;
    public double AngularVelocityDamping { get; init; } = 0.0005;
    public int MaxSteps { get; init; } = 2_000_000;
    public bool EnforceStabilityEstimate { get; init; }
    public bool EnableDeformableContact { get; init; } = true;
    public int StepCount
    {
        get
        {
            var steps = (int)Math.Ceiling(DurationSeconds / TimeStepSeconds);
            return Math.Min(steps, MaxSteps);
        }
    }

    public void Validate()
    {
        if (TimeStepSeconds <= 0.0)
            throw new InvalidOperationException("Time step must be positive.");
        if (DurationSeconds <= 0.0)
            throw new InvalidOperationException("Duration must be positive.");
        if (SnapshotStride <= 0)
            throw new InvalidOperationException("Snapshot stride must be positive.");
        if (TelemetryStride <= 0)
            throw new InvalidOperationException("Telemetry stride must be positive.");
        if (MaxSteps <= 0)
            throw new InvalidOperationException("Max steps must be positive.");
        if (GlobalVelocityDamping is < 0.0 or >= 1.0)
            throw new InvalidOperationException("Velocity damping must be in [0, 1).");
        if (AngularVelocityDamping is < 0.0 or >= 1.0)
            throw new InvalidOperationException("Angular damping must be in [0, 1).");
    }
}
