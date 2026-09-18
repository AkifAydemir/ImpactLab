using System.Text;
using ImpactLab.Core.Visualization;

namespace ImpactLab.Core.Reporting;

public static class ReportPackageBuilder
{
    public static EngineeringReportPackage Build(
        EngineeringReport report,
        IEnumerable<ChartDocument>? charts = null
    )
    {
        var p = new EngineeringReportPackage { Report = report };
        if (charts is not null)
            p.Charts.AddRange(charts);
        p.Metadata["GeneratedUtc"] = DateTimeOffset.UtcNow.ToString("O");
        for (var i = 0; i < p.Charts.Count; i++)
        {
            var svg = SvgChartExporter.Export(p.Charts[i]);
            p.Artifacts.Add(
                new ReportArtifact(
                    $"chart-{i + 1}.svg",
                    "image/svg+xml",
                    Encoding.UTF8.GetBytes(svg),
                    p.Charts[i].Title
                )
            );
        }
        return p;
    }
}
