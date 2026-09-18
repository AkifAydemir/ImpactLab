namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadBody(
    CadEntityId Id,
    string Name,
    IReadOnlyList<CadEntityId> Faces,
    bool IsSolid,
    double VolumeMeters3
);
