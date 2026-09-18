namespace ImpactLab.Core.Continuum;

public sealed record ContinuumMeshDiagnostics(
    int Nodes,
    int Elements,
    int SurfaceTriangles,
    double MinVolume,
    double MaxAspectRatio,
    int PoorElements
)
{
    public static ContinuumMeshDiagnostics Analyze(TetrahedralMesh mesh, double poorAspect = 8.0)
    {
        var q = mesh.Elements.Select(e => TetraQuality.Evaluate(mesh, e)).ToArray();
        var surface = TetrahedralSurfaceExtractor.Extract(mesh);
        return new(
            mesh.Nodes.Count,
            mesh.Elements.Count,
            surface.Triangles.Count,
            q.Min(x => x.Volume),
            q.Max(x => x.AspectRatio),
            q.Count(x => x.AspectRatio > poorAspect)
        );
    }
}
