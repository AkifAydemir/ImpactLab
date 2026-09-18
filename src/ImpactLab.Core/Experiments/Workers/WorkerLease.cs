namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerLease(
    string LeaseId,
    DateTimeOffset IssuedUtc,
    DateTimeOffset ExpiresUtc
)
{
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresUtc;
}
