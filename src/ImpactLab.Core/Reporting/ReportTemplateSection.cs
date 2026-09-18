namespace ImpactLab.Core.Reporting;

public sealed record ReportTemplateSection(
    string Id,
    string Title,
    bool Enabled = true,
    int Order = 0,
    IReadOnlyList<string>? FieldIds = null
);
