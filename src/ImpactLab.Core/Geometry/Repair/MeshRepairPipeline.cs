using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Geometry.Repair;

public sealed class MeshRepairPipeline
{
    public (TriangleMeshAsset Asset, MeshRepairReport Report) Repair(
        TriangleMeshAsset input,
        MeshRepairOptions o
    )
    {
        var a = input;
        var issues = new List<MeshRepairIssue>();
        var removed = 0;
        var welded = 0;
        var patched = 0;
        if (o.RemoveDegenerate)
        {
            a = DegenerateTriangleRemover.Apply(a, o.DegenerateAreaTolerance, out removed);
            if (removed > 0)
                issues.Add(new("degenerate", "Removed degenerate triangles.", removed, "Warning"));
        }
        if (o.WeldVertices)
        {
            a = VertexWelder.Apply(a, o.WeldToleranceMeters, out welded);
            if (welded > 0)
                issues.Add(new("weld", "Welded duplicate vertices.", welded, "Info"));
        }
        var loops = BoundaryLoopFinder.Find(a);
        if (o.PatchSmallHoles && loops.Count > 0)
        {
            a = HolePatchTriangulator.Patch(a, loops, o.MaxHoleEdges, out patched);
            if (patched > 0)
                issues.Add(new("holes", "Patched small boundary holes.", patched, "Info"));
        }
        var remaining = BoundaryLoopFinder.Find(a).Count;
        return (
            a,
            new(
                input.Vertices.Count,
                input.Indices.Count / 3,
                a.Vertices.Count,
                a.Indices.Count / 3,
                issues,
                remaining == 0
            )
        );
    }
}
