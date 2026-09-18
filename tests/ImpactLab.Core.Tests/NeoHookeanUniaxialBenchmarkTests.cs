using ImpactLab.Core.Verification.Benchmarks;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NeoHookeanUniaxialBenchmarkTests
{
    [Fact]
    public void AnalyticalStatePasses() =>
        Assert.True(new NeoHookeanUniaxialBenchmark().Evaluate().Passed);
}
