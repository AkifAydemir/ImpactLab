namespace ImpactLab.Core.Reporting;

public sealed record ReportArtifactManifest(
    string ReportId,
    string TemplateId,
    string ScenarioFingerprint,
    IReadOnlyList<string> Files,
    DateTimeOffset CreatedUtc,
    string SoftwareVersion
);
