using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PiecewiseHardeningTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(
            150.0,
            new PiecewiseIsotropicHardening([new(0, 100), new(1, 200)]).YieldStress(.5),
            8
        );
    }
}
