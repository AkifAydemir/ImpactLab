namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalCadBodyArtifact(
    string Id,
    string Name,
    IReadOnlyList<string> Faces,
    bool IsSolid,
    double VolumeMeters3
);
