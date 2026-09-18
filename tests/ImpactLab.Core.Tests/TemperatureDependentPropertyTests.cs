using ImpactLab.Core.Thermal;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class TemperatureDependentPropertyTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(
            15,
            new TemperatureDependentProperty([new(100, 10), new(200, 20)]).Evaluate(150),
            8
        );
    }
}
