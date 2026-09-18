namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalMesherArtifactManifest(
    string RequestPath,
    string RequestSha256,
    string? ResultPath,
    string? ResultSha256
);
