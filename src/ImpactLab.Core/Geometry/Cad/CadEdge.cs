namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadEdge(
    CadEntityId Id,
    CadEntityId StartVertex,
    CadEntityId EndVertex,
    IReadOnlyList<CadEntityId> AdjacentFaces,
    bool IsSeam
);
