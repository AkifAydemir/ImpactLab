namespace ImpactLab.Core.Verification.Benchmarks;

public sealed record VerificationGateAssessment(
    bool Passed,
    int PassedCount,
    int FailedCount,
    IReadOnlyList<string> BlockingBenchmarks
);
