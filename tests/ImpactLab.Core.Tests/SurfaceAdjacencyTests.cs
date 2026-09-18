using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Contact;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SurfaceAdjacencyTests
{
    [Fact]
    public void Contract()
    {
        var t = new[]
        {
            new SurfaceTriangle(0, 1, 2, "part"),
            new SurfaceTriangle(1, 2, 3, "part"),
        };
        Assert.True(new SurfaceAdjacencyMap(t).AreAdjacent(0, 1));
    }
}
