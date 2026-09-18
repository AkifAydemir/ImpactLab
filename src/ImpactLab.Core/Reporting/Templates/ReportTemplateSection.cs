namespace ImpactLab.Core.Reporting.Templates;

public sealed record ReportTemplateSection(
    string Id,
    string Heading,
    ReportTemplateSectionKind Kind,
    string? Binding = null,
    bool Enabled = true
);

public enum ReportTemplateSectionKind
{
    ExecutiveSummary,
    Scenario,
    Materials,
    Mesh,
    Results,
    Verification,
    Charts,
    Diagnostics,
    Notes,
    CustomText,
}
