namespace ImpactLab.Core.Backends;

public interface ISimulationBackend
{
    SimulationBackendDescriptor Descriptor { get; }
    BackendRunOutput Run(
        SimulationExecutionRequest request,
        CancellationToken cancellationToken = default
    );
}
