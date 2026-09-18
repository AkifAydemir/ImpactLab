namespace ImpactLab.Core.Continuum.Meshing;

public static class TetraQualityEvaluator
{
    public static TetraQualityMetrics Evaluate(TetrahedralMesh mesh, TetraElement e)
    {
        var v = TetraElementGeometry.SignedVolume(
            mesh.Nodes[e.A].Position,
            mesh.Nodes[e.B].Position,
            mesh.Nodes[e.C].Position,
            mesh.Nodes[e.D].Position
        );
        var edges = e.EdgeLengths(mesh);
        var sum = edges.Sum(x => x * x);
        var mean = sum <= 0 ? 0 : 12 * Math.Pow(Math.Abs(v), 2.0 / 3.0) / sum;
        return new(
            Math.Abs(v),
            Math.Clamp(mean, 0, 1),
            Math.Clamp(mean * 0.9, 0, 1),
            Dihedral.Min(mesh, e),
            Dihedral.Max(mesh, e),
            v <= 0
        );
    }
}
