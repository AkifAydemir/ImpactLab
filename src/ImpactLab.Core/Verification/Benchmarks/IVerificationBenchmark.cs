namespace ImpactLab.Core.Verification.Benchmarks;

public interface IVerificationBenchmark
{
    string Id { get; }
    string Name { get; }
    VerificationBenchmarkCategory Category { get; }
    VerificationBenchmarkResult Evaluate();
}
