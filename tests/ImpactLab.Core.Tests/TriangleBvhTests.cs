using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class TriangleBvhTests
{
    [Fact]
    public void RayHitsTriangle()
    {
        var a = new TriangleMeshAsset(
            "t",
            [new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)],
            [0, 1, 2]
        );
        var bvh = new TriangleBvh(a);
        Assert.True(bvh.Intersects(new Ray3(new Vec3(.2, .2, 1), new Vec3(0, 0, -1)), out var t));
        Assert.True(t > 0);
    }
}
