namespace ImpactLab.Core.Backends;

public sealed class SimulationBackendRegistry
{
    private readonly Dictionary<string, Func<ISimulationBackend>> _factories = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(string id, Func<ISimulationBackend> factory)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Backend id cannot be empty.", nameof(id));
        _factories[id] = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public ISimulationBackend Create(string id) =>
        _factories.TryGetValue(id, out var f)
            ? f()
            : throw new KeyNotFoundException($"Unknown simulation backend: {id}");

    public IReadOnlyList<SimulationBackendDescriptor> Describe() =>
        _factories.Values.Select(x => x().Descriptor).OrderBy(x => x.DisplayName).ToArray();

    public static SimulationBackendRegistry CreateDefault()
    {
        var r = new SimulationBackendRegistry();
        r.Register(ExplicitLatticeBackend.BackendId, () => new ExplicitLatticeBackend());
        r.Register(AdvancedLatticeBackend.BackendId, () => new AdvancedLatticeBackend());
        r.Register(TetraContinuumBackend.BackendId, () => new TetraContinuumBackend());
        r.Register(
            FiniteStrainContinuumBackend.BackendId,
            () => new FiniteStrainContinuumBackend()
        );
        return r;
    }
}
