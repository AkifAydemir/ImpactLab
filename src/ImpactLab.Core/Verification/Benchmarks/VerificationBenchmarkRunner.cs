namespace ImpactLab.Core.Verification.Benchmarks;

public static class VerificationBenchmarkRunner
{
    public static VerificationBenchmarkSuiteResult Run(
        IEnumerable<IVerificationBenchmark>? benchmarks = null
    )
    {
        var rows = new List<VerificationBenchmarkResult>();
        foreach (var benchmark in benchmarks ?? VerificationBenchmarkCatalog.BuiltIns)
        {
            try
            {
                rows.Add(benchmark.Evaluate());
            }
            catch (Exception ex)
            {
                rows.Add(
                    new(
                        benchmark.Id,
                        benchmark.Name,
                        benchmark.Category,
                        [],
                        "Benchmark execution failed before evidence could be produced.",
                        ex.Message
                    )
                );
            }
        }
        return new(DateTimeOffset.UtcNow, rows);
    }
}
