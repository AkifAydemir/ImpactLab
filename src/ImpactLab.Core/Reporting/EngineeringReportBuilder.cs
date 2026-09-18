using System.Globalization;
using ImpactLab.Core.Analysis;

namespace ImpactLab.Core.Reporting;

public static class EngineeringReportBuilder
{
    public static EngineeringReportDocument Build(
        string scenarioName,
        string backend,
        BackendResultSummary summary
    )
    {
        var culture = CultureInfo.InvariantCulture;
        var metrics = new List<ReportMetric>
        {
            new("Domain", summary.DomainKind),
            new(
                "Maximum displacement",
                summary.Compatibility.MaxDisplacementMeters.ToString("G6", culture),
                "m"
            ),
            new(
                "Peak contact force",
                summary.Compatibility.PeakContactForceN.ToString("G6", culture),
                "N"
            ),
        };
        if (summary.Continuum is not null)
        {
            metrics.Add(new("Tetrahedra", summary.Continuum.Elements.ToString(culture)));
            metrics.Add(
                new(
                    "Peak von Mises",
                    summary.Continuum.PeakVonMisesPa.ToString("G6", culture),
                    "Pa"
                )
            );
            metrics.Add(
                new(
                    "Maximum temperature",
                    summary.Continuum.MaxTemperatureKelvin.ToString("F2", culture),
                    "K"
                )
            );
        }
        return new(
            "ImpactLab Engineering Report",
            scenarioName,
            backend,
            DateTimeOffset.UtcNow.ToString("O", culture),
            [new("Run summary", "Generated from immutable backend results.", metrics, [])]
        );
    }
}
