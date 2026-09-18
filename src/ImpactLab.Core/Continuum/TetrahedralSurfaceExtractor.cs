namespace ImpactLab.Core.Continuum;

public static class TetrahedralSurfaceExtractor
{
    public static SurfaceMesh Extract(TetrahedralMesh mesh)
    {
        var faces = new Dictionary<(int, int, int), (int Count, SurfaceTriangle Face)>();
        foreach (var e in mesh.Elements)
        {
            Add(e.A, e.C, e.B, e.PartId);
            Add(e.A, e.B, e.D, e.PartId);
            Add(e.B, e.C, e.D, e.PartId);
            Add(e.C, e.A, e.D, e.PartId);
        }
        return new(mesh, faces.Values.Where(x => x.Count == 1).Select(x => x.Face).ToArray());
        void Add(int a, int b, int c, string part)
        {
            var sorted = new[] { a, b, c };
            Array.Sort(sorted);
            var key = (sorted[0], sorted[1], sorted[2]);
            if (faces.TryGetValue(key, out var v))
                faces[key] = (v.Count + 1, v.Face);
            else
                faces[key] = (1, new SurfaceTriangle(a, b, c, part));
        }
    }
}
