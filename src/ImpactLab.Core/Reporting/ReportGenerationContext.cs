namespace ImpactLab.Core.Reporting;

public sealed record ReportGenerationContext(
    string ScenarioName,
    string ScenarioFingerprint,
    string BackendId,
    string SoftwareVersion,
    DateTimeOffset GeneratedUtc,
    IReadOnlyDictionary<string, string> Metadata
);
