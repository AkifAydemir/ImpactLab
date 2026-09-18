namespace ImpactLab.Core.Continuum;

public static class TetraMeshAssembler
{
    public static TetrahedralMesh Combine(IReadOnlyList<TetrahedralMesh> meshes)
    {
        var nodes = new List<TetraNode>();
        var elements = new List<TetraElement>();
        foreach (var mesh in meshes)
        {
            var offset = nodes.Count;
            foreach (var n in mesh.Nodes)
                nodes.Add(n with { Id = nodes.Count });
            foreach (var e in mesh.Elements)
                elements.Add(
                    new TetraElement(
                        elements.Count,
                        e.A + offset,
                        e.B + offset,
                        e.C + offset,
                        e.D + offset,
                        e.PartId,
                        e.Material
                    )
                );
        }
        return new(nodes, elements);
    }
}
