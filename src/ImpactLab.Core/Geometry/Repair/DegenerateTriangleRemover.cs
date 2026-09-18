using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Repair;

public static class DegenerateTriangleRemover
{
    public static TriangleMeshAsset Apply(TriangleMeshAsset a, double tol, out int removed)
    {
        var idx = new List<int>();
        removed = 0;
        for (var i = 0; i < a.Indices.Count; i += 3)
        {
            var p = a.Vertices[a.Indices[i]];
            var q = a.Vertices[a.Indices[i + 1]];
            var r = a.Vertices[a.Indices[i + 2]];
            if (Vec3.Cross(q - p, r - p).Length * 0.5 <= tol)
            {
                removed++;
                continue;
            }
            idx.Add(a.Indices[i]);
            idx.Add(a.Indices[i + 1]);
            idx.Add(a.Indices[i + 2]);
        }
        return new TriangleMeshAsset(a.Name, a.Vertices.ToArray(), idx.ToArray());
    }
}
