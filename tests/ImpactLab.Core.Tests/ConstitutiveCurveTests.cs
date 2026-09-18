using ImpactLab.Core.Materials;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ConstitutiveCurveTests
{
    [Fact]
    public void InterpolatesBetweenPoints()
    {
        var c = new ConstitutiveCurve([new(0, 0), new(0.01, 100)]);
        Assert.Equal(50, c.EvaluateStress(0.005), 8);
    }

    [Fact]
    public void ClampsOutsideDomain()
    {
        var c = new ConstitutiveCurve([new(0, 0), new(1, 10)]);
        Assert.Equal(0, c.EvaluateStress(-1));
        Assert.Equal(10, c.EvaluateStress(2));
    }
}
