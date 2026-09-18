using ImpactLab.Core.Verification.Benchmarks;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class VerificationBenchmarkRunnerTests
{
    [Fact]
    public void CanonicalBenchmarksProduceEvidence()
    {
        var result = VerificationBenchmarkRunner.Run();
        Assert.NotEmpty(result.Benchmarks);
        Assert.All(result.Benchmarks, x => Assert.False(string.IsNullOrWhiteSpace(x.Evidence)));
    }
}
