namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadFace(
    CadEntityId Id,
    IReadOnlyList<CadEntityId> BoundaryEdges,
    bool Reversed,
    string SurfaceKind,
    double AreaMeters2
);
