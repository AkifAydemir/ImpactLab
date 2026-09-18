using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public sealed class TetrahedralMesh
{
    public TetrahedralMesh(IReadOnlyList<TetraNode> nodes, IReadOnlyList<TetraElement> elements)
    {
        Nodes = nodes;
        Elements = elements;
        Validate();
        Bounds = BoundingBox3.FromPoints(nodes.Select(x => x.Position));
    }

    public IReadOnlyList<TetraNode> Nodes { get; }
    public IReadOnlyList<TetraElement> Elements { get; }
    public BoundingBox3 Bounds { get; }

    public TetrahedralMesh WithNodePositions(IReadOnlyList<Vec3> positions)
    {
        if (positions.Count != Nodes.Count)
            throw new ArgumentException(
                "Position count must match the mesh node count.",
                nameof(positions)
            );
        return new TetrahedralMesh(
            Nodes.Select((node, index) => node with { Position = positions[index] }).ToArray(),
            Elements
        );
    }

    public void Validate()
    {
        if (Nodes.Count < 4)
            throw new InvalidOperationException("Tetrahedral mesh needs at least four nodes.");
        foreach (var e in Elements)
        {
            foreach (var id in e.Nodes())
                if (id < 0 || id >= Nodes.Count)
                    throw new InvalidOperationException(
                        $"Element {e.Id} references invalid node {id}."
                    );
            if (
                TetraElementGeometry.SignedVolume(
                    Nodes[e.A].Position,
                    Nodes[e.B].Position,
                    Nodes[e.C].Position,
                    Nodes[e.D].Position
                ) <= 1e-18
            )
                throw new InvalidOperationException($"Element {e.Id} is degenerate or inverted.");
        }
    }
}
