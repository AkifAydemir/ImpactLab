using ImpactLab.Core.Visualization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NiceTickGeneratorTests
{
    [Fact]
    public void ProducesOrderedTicks()
    {
        var t = NiceTickGenerator.Generate(-.2, 1.1);
        Assert.True(t.Count >= 3);
        Assert.True(t.Zip(t.Skip(1)).All(x => x.First.Value < x.Second.Value));
    }
}
