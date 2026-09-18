namespace ImpactLab.Core.Experiments.Workers;

public sealed class WorkerLeaseRegistry
{
    private readonly Dictionary<string, (string WorkerId, WorkerLease Lease)> _leases = new(
        StringComparer.OrdinalIgnoreCase
    );

    public WorkerLease Issue(string workerId, TimeSpan duration)
    {
        var now = DateTimeOffset.UtcNow;
        var lease = new WorkerLease(Guid.NewGuid().ToString("N"), now, now + duration);
        _leases[lease.LeaseId] = (workerId, lease);
        return lease;
    }

    public bool Validate(string workerId, string leaseId, DateTimeOffset now) =>
        _leases.TryGetValue(leaseId, out var x)
        && string.Equals(x.WorkerId, workerId, StringComparison.OrdinalIgnoreCase)
        && !x.Lease.IsExpired(now);

    public void Release(string leaseId) => _leases.Remove(leaseId);

    public int ReapExpired(DateTimeOffset now)
    {
        var ids = _leases.Where(x => x.Value.Lease.IsExpired(now)).Select(x => x.Key).ToArray();
        foreach (var id in ids)
            _leases.Remove(id);
        return ids.Length;
    }
}
