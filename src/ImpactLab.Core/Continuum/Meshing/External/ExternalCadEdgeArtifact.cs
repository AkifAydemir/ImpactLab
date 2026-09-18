namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalCadEdgeArtifact(
    string Id,
    string StartVertex,
    string EndVertex,
    IReadOnlyList<string> AdjacentFaces,
    bool IsSeam
);
