using ImpactLab.Core.Diagnostics;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Backends;

public sealed class SimulationExecutionContext
{
    public MaterialExecutionContext? Materials { get; init; }
    public PerformanceTrace Performance { get; init; } = new();
    public double DefaultTemperatureKelvin { get; init; } = 293.15;
    public bool CaptureNumericalHealth { get; init; } = true;
    public Action<NumericalHealthSample>? HealthSink { get; init; }
}
