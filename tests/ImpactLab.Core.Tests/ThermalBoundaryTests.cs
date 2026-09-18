using ImpactLab.Core.Thermal;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ThermalBoundaryTests
{
    [Fact]
    public void Contract()
    {
        var rhs = new double[1];
        var d = new double[1];
        ThermalBoundaryAssembler.AddConvection(new([0], 10, 300, 1), new[] { 293d }, rhs, d);
        Assert.Equal(10, d[0], 6);
    }
}
