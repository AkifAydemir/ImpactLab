using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class AdaptiveMeshPlannerTests
{
    [Fact]
    public void BoundaryRefinementCreatesMultipleDepths()
    {
        var g = new SphereSpec("s", Vec3.Zero, 0.1);
        var m = MaterialLibrary.All[0];
        var settings = new MeshingSettings
        {
            Mode = MeshingMode.AdaptiveLattice,
            BaseCellSizeMeters = 0.05,
            MinimumCellSizeMeters = 0.0125,
            MaximumRefinementDepth = 3,
        };
        var plan = new AdaptiveMeshPlanner().Plan(g, m, [], settings);
        Assert.NotEmpty(plan.Leaves);
        Assert.True(plan.MaximumDepth > 0);
        Assert.True(plan.MinimumCellSize < plan.MaximumCellSize);
    }
}
