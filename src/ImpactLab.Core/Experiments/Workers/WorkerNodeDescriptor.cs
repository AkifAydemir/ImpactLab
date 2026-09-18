namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerNodeDescriptor(
    string WorkerId,
    WorkerNodeStatus Status,
    WorkerCapabilities Capabilities,
    DateTimeOffset LastSeenUtc,
    int ActiveLeases,
    int Capacity
)
{
    public bool CanAccept =>
        Status is WorkerNodeStatus.Ready or WorkerNodeStatus.Busy
        && ActiveLeases < Math.Max(1, Capacity);
}
