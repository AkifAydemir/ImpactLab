namespace ImpactLab.Core.Backends;

public sealed record SimulationBackendDescriptor(
    string Id,
    string DisplayName,
    Version Version,
    SimulationBackendCapabilities Capabilities,
    string Description
)
{
    public bool Supports(SimulationBackendCapabilities capability) =>
        (Capabilities & capability) == capability;
}
