namespace ImpactLab.Core.Verification.Benchmarks;

public static class VerificationGateEvaluator
{
    public static VerificationGateAssessment Evaluate(VerificationBenchmarkSuiteResult result)
    {
        var failed = result.Benchmarks.Where(x => !x.Passed).Select(x => x.Id).ToArray();
        return new(
            failed.Length == 0,
            result.Benchmarks.Count - failed.Length,
            failed.Length,
            failed
        );
    }
}
