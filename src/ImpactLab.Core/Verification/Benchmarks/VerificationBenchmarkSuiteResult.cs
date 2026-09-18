namespace ImpactLab.Core.Verification.Benchmarks;

public sealed record VerificationBenchmarkSuiteResult(
    DateTimeOffset ExecutedUtc,
    IReadOnlyList<VerificationBenchmarkResult> Benchmarks
)
{
    public bool Passed => Benchmarks.All(x => x.Passed);
    public IReadOnlyList<VerificationBenchmarkResult> Failed =>
        Benchmarks.Where(x => !x.Passed).ToArray();
}
