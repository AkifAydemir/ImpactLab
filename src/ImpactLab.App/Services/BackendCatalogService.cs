using ImpactLab.Core.Backends;

namespace ImpactLab.App.Services;

public sealed class BackendCatalogService
{
    public BackendCatalogService(SimulationBackendRegistry? registry = null) =>
        Registry = registry ?? SimulationBackendRegistry.CreateDefault();

    public SimulationBackendRegistry Registry { get; }
    public IReadOnlyList<SimulationBackendDescriptor> Backends => Registry.Describe();
}
