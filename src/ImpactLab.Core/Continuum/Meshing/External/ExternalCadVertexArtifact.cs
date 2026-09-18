namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalCadVertexArtifact(
    string Id,
    double X,
    double Y,
    double Z,
    double ToleranceMeters
);
