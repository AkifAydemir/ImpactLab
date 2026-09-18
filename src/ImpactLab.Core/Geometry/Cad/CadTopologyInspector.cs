namespace ImpactLab.Core.Geometry.Cad;

public static class CadTopologyInspector
{
    public static IReadOnlyList<CadTopologyIssue> Inspect(
        CadModel m,
        double shortEdge = 1e-5,
        double smallFace = 1e-8
    )
    {
        var issues = new List<CadTopologyIssue>();
        foreach (var e in m.Edges)
        {
            if (e.AdjacentFaces.Count > 2)
                issues.Add(
                    new(
                        CadTopologyIssueKind.NonManifoldEdge,
                        "Edge has more than two adjacent faces.",
                        [e.Id],
                        1
                    )
                );
            if (e.AdjacentFaces.Count < 2)
                issues.Add(
                    new(CadTopologyIssueKind.OpenShell, "Boundary edge found.", [e.Id], 0.6)
                );
            var a = m.Vertices.First(v => v.Id == e.StartVertex).Position;
            var b = m.Vertices.First(v => v.Id == e.EndVertex).Position;
            if ((a - b).Length < shortEdge)
                issues.Add(new(CadTopologyIssueKind.ShortEdge, "Short edge.", [e.Id], 0.4));
        }
        foreach (var f in m.Faces.Where(x => x.AreaMeters2 < smallFace))
            issues.Add(new(CadTopologyIssueKind.SmallFace, "Small face.", [f.Id], 0.3));
        return issues;
    }
}
