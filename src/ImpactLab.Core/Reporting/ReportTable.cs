namespace ImpactLab.Core.Reporting;

public sealed record ReportTable(
    string Title,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows
);
