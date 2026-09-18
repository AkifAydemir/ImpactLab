namespace ImpactLab.Core.Reporting.Templates;

public static class ReportTemplateCatalog
{
    public static IReadOnlyList<ReportTemplate> BuiltIns =>
        [
            new(
                "engineering-standard",
                "Engineering Standard",
                "ImpactLab Engineering Analysis",
                [
                    new("summary", "Executive Summary", ReportTemplateSectionKind.ExecutiveSummary),
                    new("scenario", "Scenario", ReportTemplateSectionKind.Scenario),
                    new("results", "Results", ReportTemplateSectionKind.Results),
                    new("diagnostics", "Diagnostics", ReportTemplateSectionKind.Diagnostics),
                ]
            ),
            new(
                "verification",
                "Verification Report",
                "ImpactLab Verification Report",
                [
                    new("summary", "Summary", ReportTemplateSectionKind.ExecutiveSummary),
                    new("verification", "Verification", ReportTemplateSectionKind.Verification),
                    new("charts", "Charts", ReportTemplateSectionKind.Charts),
                ]
            ),
        ];
}
