namespace ImpactLab.Core.Continuum;

public readonly record struct TetraQuality(
    double Volume,
    double MinEdge,
    double MaxEdge,
    double AspectRatio
)
{
    public static TetraQuality Evaluate(TetrahedralMesh mesh, TetraElement e)
    {
        var p = new[]
        {
            mesh.Nodes[e.A].Position,
            mesh.Nodes[e.B].Position,
            mesh.Nodes[e.C].Position,
            mesh.Nodes[e.D].Position,
        };
        var min = TetraElementGeometry.EdgeMin(p);
        var max = TetraElementGeometry.EdgeMax(p);
        return new(
            Math.Abs(TetraElementGeometry.SignedVolume(p[0], p[1], p[2], p[3])),
            min,
            max,
            min <= 1e-18 ? double.PositiveInfinity : max / min
        );
    }
}
