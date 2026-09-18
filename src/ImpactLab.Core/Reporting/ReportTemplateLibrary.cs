namespace ImpactLab.Core.Reporting;

public static class ReportTemplateLibrary
{
    public static IReadOnlyList<EngineeringReportTemplate> All { get; } =
    [
        new(
            "engineering-summary",
            "Engineering Summary",
            [
                new("overview", "Overview", true, 0),
                new("inputs", "Inputs", true, 1),
                new("results", "Results", true, 2),
                new("events", "Events", true, 3),
                new("limitations", "Limitations", true, 4),
            ]
        ),
        new(
            "verification",
            "Verification Report",
            [
                new("overview", "Overview"),
                new("verification", "Verification"),
                new("plots", "Plots"),
                new("provenance", "Provenance"),
            ]
        ),
    ];
}
