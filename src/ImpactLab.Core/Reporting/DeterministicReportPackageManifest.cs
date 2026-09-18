namespace ImpactLab.Core.Reporting;

public sealed record DeterministicReportPackageManifest(
    int SchemaVersion,
    string ReportId,
    string TemplateId,
    string ScenarioFingerprint,
    string BackendId,
    string SoftwareVersion,
    DateTimeOffset GeneratedUtc,
    IReadOnlyList<ReportPackageFileEntry> Files
);
