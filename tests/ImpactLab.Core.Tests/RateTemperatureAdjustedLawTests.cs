using ImpactLab.Core.Materials;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class RateTemperatureAdjustedLawTests
{
    [Fact]
    public void HigherTemperatureSoftensResponse()
    {
        var p = new MaterialProfile(MaterialLibrary.All[0]);
        var law = new RateTemperatureAdjustedLaw(new LegacyBilinearConstitutiveLaw());
        var cold = law.Evaluate(p, new MaterialStatePoint(.001, 1, 293.15));
        var hot = law.Evaluate(p, new MaterialStatePoint(.001, 1, 700));
        Assert.True(Math.Abs(hot.StressPa) < Math.Abs(cold.StressPa));
    }
}
