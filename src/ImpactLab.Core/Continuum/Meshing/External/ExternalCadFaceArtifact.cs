namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalCadFaceArtifact(
    string Id,
    IReadOnlyList<string> BoundaryEdges,
    bool Reversed,
    string SurfaceKind,
    double AreaMeters2
);
