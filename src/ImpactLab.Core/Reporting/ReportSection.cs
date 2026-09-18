namespace ImpactLab.Core.Reporting;

public sealed class ReportSection
{
    public string Title { get; init; } = "Section";
    public List<string> Paragraphs { get; } = [];
    public List<ReportTable> Tables { get; } = [];
}
