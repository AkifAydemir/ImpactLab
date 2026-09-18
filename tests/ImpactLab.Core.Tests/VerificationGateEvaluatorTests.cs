using ImpactLab.Core.Verification.Benchmarks;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class VerificationGateEvaluatorTests
{
    [Fact]
    public void CatalogAssessmentIsDeterministic()
    {
        var r = VerificationBenchmarkRunner.Run();
        var a = VerificationGateEvaluator.Evaluate(r);
        Assert.Equal(r.Benchmarks.Count, a.PassedCount + a.FailedCount);
    }
}
