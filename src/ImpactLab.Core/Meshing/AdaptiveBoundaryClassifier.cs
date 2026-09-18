using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public static class AdaptiveBoundaryClassifier
{
    public static IReadOnlyList<int> FindBoundaryNodes(
        GeometrySpec geometry,
        IReadOnlyList<MeshNode> nodes
    )
    {
        var ids = new List<int>();
        foreach (var n in nodes)
        {
            var h = n.CellSize * 0.55;
            var probes = new[]
            {
                new Vec3(h, 0, 0),
                new Vec3(-h, 0, 0),
                new Vec3(0, h, 0),
                new Vec3(0, -h, 0),
                new Vec3(0, 0, h),
                new Vec3(0, 0, -h),
            };
            if (probes.Any(d => !geometry.Contains(n.RestPosition + d)))
                ids.Add(n.Id);
        }
        return ids;
    }
}
