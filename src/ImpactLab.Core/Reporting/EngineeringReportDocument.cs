namespace ImpactLab.Core.Reporting;

public sealed record ReportMetric(string Name, string Value, string? Unit = null);

public sealed record EngineeringReportSection(
    string Title,
    string? Narrative,
    IReadOnlyList<ReportMetric> Metrics,
    IReadOnlyList<ReportTable> Tables
);

public sealed record EngineeringReportDocument(
    string Title,
    string ScenarioName,
    string Backend,
    string GeneratedUtc,
    IReadOnlyList<EngineeringReportSection> Sections
);
