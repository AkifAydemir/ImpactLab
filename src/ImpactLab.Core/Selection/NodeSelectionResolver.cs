using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Selection;

public static class NodeSelectionResolver
{
    public static NodeSelection Resolve(SimulationMesh mesh, string id, NodeSelectionQuery query)
    {
        query.Validate();
        var surface = query.SurfaceOnly ? SurfaceNodeExtractor.Build(mesh) : null;
        var surfaceIds =
            query.SurfaceOnly && query.PartId is not null
                ? new HashSet<int>(surface!.Get(query.PartId))
            : query.SurfaceOnly ? surface!.ByPart.Values.SelectMany(x => x).ToHashSet()
            : null;
        var normal = query.UsesPlane ? query.PlaneNormal.Normalized() : default;
        var ids = new List<int>();
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var node = mesh.Nodes[i];
            if (
                query.PartId is not null
                && !string.Equals(node.PartId, query.PartId, StringComparison.OrdinalIgnoreCase)
            )
                continue;
            if (surfaceIds is not null && !surfaceIds.Contains(i))
                continue;
            if (query.Bounds is { } bounds && !bounds.Contains(node.RestPosition))
                continue;
            if (query.UsesPlane)
            {
                var distance = Math.Abs(
                    ImpactLab.Core.Mathematics.Vec3.Dot(node.RestPosition, normal)
                        - query.PlaneOffset
                );
                if (distance > query.PlaneToleranceMeters)
                    continue;
            }
            ids.Add(i);
        }
        return new NodeSelection(id, ids);
    }
}
