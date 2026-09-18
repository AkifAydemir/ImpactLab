using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public static class StructuredTetraMesher
{
    private static readonly int[][] Pattern =
    [
        [0, 1, 3, 4],
        [1, 2, 3, 6],
        [1, 3, 4, 6],
        [1, 4, 5, 6],
        [3, 4, 6, 7],
    ];

    public static TetrahedralMesh Build(
        string partId,
        GeometrySpec geometry,
        MaterialDefinition material,
        StructuredTetraMesherSettings settings
    )
    {
        settings.Validate();
        var b = geometry.Bounds;
        var h = settings.EdgeLengthMeters;
        var nx = Math.Clamp((int)Math.Ceiling(b.Size.X / h), 1, settings.MaxCellsPerAxis);
        var ny = Math.Clamp((int)Math.Ceiling(b.Size.Y / h), 1, settings.MaxCellsPerAxis);
        var nz = Math.Clamp((int)Math.Ceiling(b.Size.Z / h), 1, settings.MaxCellsPerAxis);
        var dx = b.Size.X / nx;
        var dy = b.Size.Y / ny;
        var dz = b.Size.Z / nz;
        var nodeMap = new Dictionary<(int, int, int), int>();
        var nodes = new List<TetraNode>();
        var elements = new List<TetraElement>();
        int Node(int x, int y, int z)
        {
            var key = (x, y, z);
            if (nodeMap.TryGetValue(key, out var id))
                return id;
            var p = new Vec3(b.Min.X + x * dx, b.Min.Y + y * dy, b.Min.Z + z * dz);
            id = nodes.Count;
            nodeMap[key] = id;
            nodes.Add(new TetraNode(id, p, partId, material));
            return id;
        }
        for (var z = 0; z < nz; z++)
        for (var y = 0; y < ny; y++)
        for (var x = 0; x < nx; x++)
        {
            var center = new Vec3(
                b.Min.X + (x + 0.5) * dx,
                b.Min.Y + (y + 0.5) * dy,
                b.Min.Z + (z + 0.5) * dz
            );
            if (!geometry.Contains(center))
                continue;
            var ids = new[]
            {
                Node(x, y, z),
                Node(x + 1, y, z),
                Node(x + 1, y + 1, z),
                Node(x, y + 1, z),
                Node(x, y, z + 1),
                Node(x + 1, y, z + 1),
                Node(x + 1, y + 1, z + 1),
                Node(x, y + 1, z + 1),
            };
            foreach (var t in Pattern)
            {
                var a = ids[t[0]];
                var bb = ids[t[1]];
                var c = ids[t[2]];
                var dd = ids[t[3]];
                var va = TetraElementGeometry.SignedVolume(
                    nodes[a].Position,
                    nodes[bb].Position,
                    nodes[c].Position,
                    nodes[dd].Position
                );
                if (va < 0)
                    (bb, c) = (c, bb);
                elements.Add(new TetraElement(elements.Count, a, bb, c, dd, partId, material));
            }
        }
        if (elements.Count == 0)
            throw new InvalidOperationException(
                $"Continuum mesher produced no tetrahedra for {partId}."
            );
        return new TetrahedralMesh(nodes, elements);
    }
}
