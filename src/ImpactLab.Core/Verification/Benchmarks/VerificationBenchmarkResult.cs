namespace ImpactLab.Core.Verification.Benchmarks;

public sealed record VerificationBenchmarkResult(
    string Id,
    string Name,
    VerificationBenchmarkCategory Category,
    IReadOnlyList<VerificationBenchmarkMetric> Metrics,
    string Evidence,
    string? Error = null
)
{
    public bool Passed => Error is null && Metrics.Count > 0 && Metrics.All(x => x.Passed);
}
