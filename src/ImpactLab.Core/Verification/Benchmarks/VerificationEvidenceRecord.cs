namespace ImpactLab.Core.Verification.Benchmarks;

public sealed record VerificationEvidenceRecord(
    string BenchmarkId,
    bool Passed,
    DateTimeOffset ExecutedUtc,
    string Runtime,
    string Machine,
    IReadOnlyList<VerificationBenchmarkMetric> Metrics,
    string Evidence,
    string? Error
);
