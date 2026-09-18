using ImpactLab.Core.Experiments.Optimization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class GaussianProcessTests
{
    private static readonly double[] Origin = [0d];

    [Fact]
    public void Contract()
    {
        var g = new GaussianProcessSurrogate(new());
        g.Fit([Origin], [1d]);
        Assert.True(g.Predict(Origin).Variance >= 0);
    }
}
