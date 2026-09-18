namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalMesherRequestArtifact(
    int SchemaVersion,
    string SourceFormat,
    IReadOnlyList<ExternalCadVertexArtifact> Vertices,
    IReadOnlyList<ExternalCadEdgeArtifact> Edges,
    IReadOnlyList<ExternalCadFaceArtifact> Faces,
    IReadOnlyList<ExternalCadBodyArtifact> Bodies,
    ExternalMeshingParametersArtifact Meshing,
    IReadOnlyDictionary<string, string> Options
);
