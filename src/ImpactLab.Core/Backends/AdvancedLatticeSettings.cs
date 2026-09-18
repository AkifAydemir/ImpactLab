namespace ImpactLab.Core.Backends;

public sealed record AdvancedLatticeSettings
{
    public double NumericalViscosity { get; init; } = 0.0015;
    public double DamageRateLimitPerStep { get; init; } = 0.05;
    public double MinimumResidualStiffness { get; init; } = 0.005;
    public bool EnableCohesiveInterfaces { get; init; } = true;
    public bool EnableThermalSoftening { get; init; } = true;
    public int HealthStride { get; init; } = 50;
}
