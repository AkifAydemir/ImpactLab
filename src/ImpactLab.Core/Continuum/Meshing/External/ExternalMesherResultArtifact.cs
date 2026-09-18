namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalMesherResultArtifact(
    int SchemaVersion,
    bool Success,
    IReadOnlyList<ExternalMeshNodeArtifact> Nodes,
    IReadOnlyList<ExternalMeshElementArtifact> Elements,
    IReadOnlyDictionary<string, string> Diagnostics,
    string? Error
);
