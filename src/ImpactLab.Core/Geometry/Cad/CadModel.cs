namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadModel(
    IReadOnlyList<CadVertex> Vertices,
    IReadOnlyList<CadEdge> Edges,
    IReadOnlyList<CadFace> Faces,
    IReadOnlyList<CadBody> Bodies,
    string SourceFormat,
    string? SourcePath
);
