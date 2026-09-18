using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Repair;

public static class VertexWelder
{
    public static TriangleMeshAsset Apply(TriangleMeshAsset a, double tol, out int welded)
    {
        var map = new Dictionary<(long, long, long), int>();
        var verts = new List<Vec3>();
        var remap = new int[a.Vertices.Count];
        var inv = 1 / Math.Max(tol, 1e-15);
        welded = 0;
        for (var i = 0; i < a.Vertices.Count; i++)
        {
            var p = a.Vertices[i];
            var k = (
                (long)Math.Round(p.X * inv),
                (long)Math.Round(p.Y * inv),
                (long)Math.Round(p.Z * inv)
            );
            if (map.TryGetValue(k, out var id))
            {
                remap[i] = id;
                welded++;
            }
            else
            {
                remap[i] = verts.Count;
                map[k] = verts.Count;
                verts.Add(p);
            }
        }
        return new TriangleMeshAsset(a.Name, verts, a.Indices.Select(i => remap[i]).ToArray());
    }
}
