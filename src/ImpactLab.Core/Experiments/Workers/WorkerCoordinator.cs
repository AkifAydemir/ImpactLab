namespace ImpactLab.Core.Experiments.Workers;

public sealed class WorkerCoordinator
{
    private readonly Dictionary<string, WorkerNodeDescriptor> _nodes = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(WorkerHello hello, int capacity = 1) =>
        _nodes[hello.WorkerId] = new(
            hello.WorkerId,
            WorkerNodeStatus.Ready,
            hello.Capabilities,
            DateTimeOffset.UtcNow,
            0,
            Math.Max(1, capacity)
        );

    public void Heartbeat(WorkerHeartbeat heartbeat)
    {
        if (_nodes.TryGetValue(heartbeat.WorkerId, out var node))
            _nodes[heartbeat.WorkerId] = node with
            {
                LastSeenUtc = DateTimeOffset.UtcNow,
                Status = node.ActiveLeases == 0 ? WorkerNodeStatus.Ready : WorkerNodeStatus.Busy,
            };
    }

    public WorkerNodeDescriptor? Select(IReadOnlyCollection<string>? requiredBackends = null)
    {
        return _nodes
            .Values.Where(x => x.CanAccept)
            .Where(x =>
                requiredBackends is null
                || requiredBackends.All(b =>
                    x.Capabilities.BackendIds.Contains(b, StringComparer.OrdinalIgnoreCase)
                )
            )
            .OrderBy(x => (double)x.ActiveLeases / Math.Max(1, x.Capacity))
            .ThenByDescending(x => x.LastSeenUtc)
            .FirstOrDefault();
    }

    public IReadOnlyList<WorkerNodeDescriptor> Snapshot() =>
        _nodes.Values.OrderBy(x => x.WorkerId, StringComparer.OrdinalIgnoreCase).ToArray();
}
