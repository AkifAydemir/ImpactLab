using ImpactLab.Core.PostProcessing;

namespace ImpactLab.Core.Visualization;

public static class ProbeChartBuilder
{
    public static ChartDocument Build(ProbeSeries probe, string unit = "")
    {
        var chart = new ChartDocument
        {
            Title = probe.Definition.Name,
            XLabel = "Time (s)",
            YLabel = $"{probe.Definition.Quantity} {unit}".Trim(),
        };
        chart.Series.Add(
            new ChartSeries(
                probe.Definition.Name,
                "s",
                unit,
                probe.Samples.Select(x => new ChartPoint(x.TimeSeconds, x.Value)).ToArray()
            )
        );
        return chart;
    }
}
