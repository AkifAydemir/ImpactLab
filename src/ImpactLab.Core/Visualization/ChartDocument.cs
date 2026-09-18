namespace ImpactLab.Core.Visualization;

public sealed class ChartDocument
{
    public string Title { get; init; } = "Chart";
    public string XLabel { get; init; } = "X";
    public string YLabel { get; init; } = "Y";
    public List<ChartSeries> Series { get; } = [];
}
