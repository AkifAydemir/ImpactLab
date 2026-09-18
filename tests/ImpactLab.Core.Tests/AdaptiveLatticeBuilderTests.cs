using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class AdaptiveLatticeBuilderTests
{
    [Fact]
    public void ProducesBoundaryNodesAndSprings()
    {
        var mesh = AdaptiveLatticeBuilder.Build(
            "p",
            new BoxSpec("b", Vec3.Zero, new Vec3(.1, .1, .1)),
            MaterialLibrary.All[0],
            new MeshingSettings
            {
                Mode = MeshingMode.AdaptiveLattice,
                BaseCellSizeMeters = .05,
                MinimumCellSizeMeters = .025,
                MaximumRefinementDepth = 2,
            }
        );
        Assert.NotEmpty(mesh.Nodes);
        Assert.NotEmpty(mesh.Springs);
        Assert.True(mesh.TryGetBoundaryNodes("p", out var ids));
        Assert.NotEmpty(ids);
    }
}
