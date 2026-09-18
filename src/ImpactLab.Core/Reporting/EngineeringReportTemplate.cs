namespace ImpactLab.Core.Reporting;

public sealed record EngineeringReportTemplate(
    string Id,
    string DisplayName,
    IReadOnlyList<ReportTemplateSection> Sections,
    bool IncludeCover = true,
    bool IncludeProvenance = true,
    bool IncludeLimitations = true
);
