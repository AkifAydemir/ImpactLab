namespace ImpactLab.Core.Reporting;

public sealed record ReportArtifact(
    string Name,
    string MediaType,
    byte[] Content,
    string? Description = null
);
