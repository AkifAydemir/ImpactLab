using ImpactLab.Core.Experiments.Uncertainty;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class LatinHypercubeSamplerTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(
            10,
            new LatinHypercubeSampler()
                .Generate([new("x", DistributionKind.Uniform, 0, 1)], 10, 1)
                .Count
        );
    }
}
