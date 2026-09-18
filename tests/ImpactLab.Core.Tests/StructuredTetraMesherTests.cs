using ImpactLab.Core.Continuum;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class StructuredTetraMesherTests
{
    [Fact]
    public void BoxProducesPositiveTetrahedra()
    {
        var mesh = StructuredTetraMesher.Build(
            "p",
            new BoxSpec("b", Vec3.Zero, new Vec3(.1, .1, .1)),
            MaterialLibrary.All[0],
            new(.05)
        );
        Assert.NotEmpty(mesh.Elements);
        Assert.All(mesh.Elements, e => Assert.True(TetraQuality.Evaluate(mesh, e).Volume > 0));
    }
}
