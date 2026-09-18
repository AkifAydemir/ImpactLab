namespace ImpactLab.Core.Meshing;

public static class SurfaceNodeExtractor
{
    private static readonly (int X, int Y, int Z)[] Neighbors =
    [
        (1, 0, 0),
        (-1, 0, 0),
        (0, 1, 0),
        (0, -1, 0),
        (0, 0, 1),
        (0, 0, -1),
    ];

    public static SurfaceNodeSet Build(SimulationMesh mesh)
    {
        var result = new SurfaceNodeSet();
        foreach (var part in mesh.Parts)
        {
            if (mesh.TryGetBoundaryNodes(part.PartId, out var pre))
            {
                result.Set(part.PartId, pre);
                continue;
            }
            var surface = new List<int>();
            for (var i = part.NodeStart; i < part.NodeEndExclusive; i++)
            {
                var node = mesh.Nodes[i];
                foreach (var n in Neighbors)
                {
                    if (
                        !mesh.TryGetNodeId(
                            part.PartId,
                            node.GridX + n.X,
                            node.GridY + n.Y,
                            node.GridZ + n.Z,
                            out _
                        )
                    )
                    {
                        surface.Add(i);
                        break;
                    }
                }
            }
            result.Set(part.PartId, surface);
        }
        return result;
    }
}
