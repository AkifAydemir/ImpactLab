using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CompositeGeometryTests
{
    [Fact]
    public void SubtractionRemovesOverlap()
    {
        var a = new BoxSpec("a", Vec3.Zero, new Vec3(2, 2, 2));
        var b = new SphereSpec("b", Vec3.Zero, .5);
        var c = new CompositeGeometrySpec("c", a, b, BooleanGeometryOperation.Subtract);
        Assert.False(c.Contains(Vec3.Zero));
        Assert.True(c.Contains(new Vec3(.8, 0, 0)));
    }
}
