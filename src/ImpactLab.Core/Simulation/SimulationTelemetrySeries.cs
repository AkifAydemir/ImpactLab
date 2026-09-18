namespace ImpactLab.Core.Simulation;

public sealed class SimulationTelemetrySeries
{
    private readonly List<SimulationTelemetrySample> _samples = [];
    public IReadOnlyList<SimulationTelemetrySample> Samples => _samples;

    public void Add(SimulationTelemetrySample sample) => _samples.Add(sample);
}
