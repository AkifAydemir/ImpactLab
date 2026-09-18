using ImpactLab.Core.Visualization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SvgChartExporterTests
{
    [Fact]
    public void EmitsSvgPolyline()
    {
        var d = new ChartDocument { Title = "x" };
        d.Series.Add(new ChartSeries("s", "s", "N", [new(0, 0), new(1, 1)]));
        var svg = SvgChartExporter.Export(d);
        Assert.Contains("<svg", svg);
        Assert.Contains("polyline", svg);
    }
}
