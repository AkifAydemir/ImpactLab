using ImpactLab.Core.Verification.Benchmarks;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PureRotationObjectivityBenchmarkTests
{
    [Fact]
    public void PureRotationPasses() =>
        Assert.True(new PureRotationObjectivityBenchmark().Evaluate().Passed);
}
