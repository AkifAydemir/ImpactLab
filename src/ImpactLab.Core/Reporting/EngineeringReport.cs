namespace ImpactLab.Core.Reporting;

public sealed class EngineeringReport
{
    public string Title { get; init; } = "ImpactLab Report";
    public string Subtitle { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public List<ReportSection> Sections { get; } = [];
}
