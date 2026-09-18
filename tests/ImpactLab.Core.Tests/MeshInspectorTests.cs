using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class MeshInspectorTests
{
    [Fact]
    public void OpenTriangleReportsBoundaryEdges()
    {
        var a = new TriangleMeshAsset("t", [Vec3.Zero, Vec3.UnitX, Vec3.UnitY], [0, 1, 2]);
        var r = MeshInspector.Analyze(a);
        Assert.Equal(3, r.BoundaryEdges);
        Assert.False(r.IsLikelyClosed);
    }
}
