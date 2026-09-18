namespace ImpactLab.Core.Geometry.Imported;

public static class MeshInspector
{
    public static MeshRepairReport Analyze(TriangleMeshAsset asset, double areaTolerance = 1e-16)
    {
        var degenerate = 0;
        var duplicates = 0;
        var seen = new HashSet<(int, int, int)>();
        var edges = new Dictionary<(int, int), int>();
        for (var i = 0; i < asset.Indices.Count; i += 3)
        {
            var a = asset.Indices[i];
            var b = asset.Indices[i + 1];
            var c = asset.Indices[i + 2];
            var key = Sort(a, b, c);
            if (!seen.Add(key))
                duplicates++;
            if (
                new Triangle3(asset.Vertices[a], asset.Vertices[b], asset.Vertices[c]).Area
                <= areaTolerance
            )
                degenerate++;
            AddEdge(edges, a, b);
            AddEdge(edges, b, c);
            AddEdge(edges, c, a);
        }
        var boundary = edges.Values.Count(x => x == 1);
        var warnings = new List<string>();
        if (degenerate > 0)
            warnings.Add($"{degenerate} degenerate triangles detected.");
        if (duplicates > 0)
            warnings.Add($"{duplicates} duplicate triangles detected.");
        if (boundary > 0)
            warnings.Add(
                $"{boundary} boundary edges detected; occupancy may be unreliable for open surfaces."
            );
        return new MeshRepairReport(
            asset.Vertices.Count,
            asset.TriangleCount,
            degenerate,
            duplicates,
            boundary,
            boundary == 0,
            warnings
        );
    }

    private static void AddEdge(Dictionary<(int, int), int> map, int a, int b)
    {
        var key = a < b ? (a, b) : (b, a);
        map[key] = map.TryGetValue(key, out var n) ? n + 1 : 1;
    }

    private static (int, int, int) Sort(int a, int b, int c)
    {
        var x = new[] { a, b, c };
        Array.Sort(x);
        return (x[0], x[1], x[2]);
    }
}
