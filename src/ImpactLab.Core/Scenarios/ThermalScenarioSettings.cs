namespace ImpactLab.Core.Scenarios;

public sealed record ThermalScenarioSettings
{
    public bool Enabled { get; init; } = false;
    public double InitialTemperatureKelvin { get; init; } = 293.15;
    public double DurationSeconds { get; init; } = 1.0;
    public double TimeStepSeconds { get; init; } = 0.05;

    public void Validate()
    {
        if (InitialTemperatureKelvin <= 0 || DurationSeconds <= 0 || TimeStepSeconds <= 0)
            throw new InvalidOperationException("Thermal settings must be positive.");
    }
}
