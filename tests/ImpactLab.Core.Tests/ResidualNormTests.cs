using ImpactLab.Core.Continuum.Nonlinear;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ResidualNormTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(5, ResidualNormEvaluator.L2(new double[] { 3, 4 }), 6);
    }
}
