using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Geometry.Repair;

public static class HolePatchTriangulator
{
    public static TriangleMeshAsset Patch(
        TriangleMeshAsset a,
        IReadOnlyList<int[]> loops,
        int maxEdges,
        out int patched
    )
    {
        var idx = a.Indices.ToList();
        patched = 0;
        foreach (var loop in loops.Where(x => x.Length - 1 <= maxEdges))
        {
            var ring = loop[^1] == loop[0] ? loop[..^1] : loop;
            if (ring.Length < 3)
                continue;
            for (var i = 1; i < ring.Length - 1; i++)
            {
                idx.Add(ring[0]);
                idx.Add(ring[i]);
                idx.Add(ring[i + 1]);
                patched++;
            }
        }
        return new TriangleMeshAsset(a.Name, a.Vertices.ToArray(), idx.ToArray());
    }
}
