namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalTetraMesherResult(
    bool Success,
    TetrahedralMesh? Mesh,
    string StandardOutput,
    string StandardError,
    TimeSpan Elapsed,
    int ExitCode,
    ExternalMesherArtifactManifest Artifacts,
    ExternalMesherValidationResult? Validation,
    string? Error = null
);
