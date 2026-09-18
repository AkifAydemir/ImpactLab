using ImpactLab.Core.Visualization;

namespace ImpactLab.Core.Reporting;

public sealed class EngineeringReportPackage
{
    public required EngineeringReport Report { get; init; }
    public List<ChartDocument> Charts { get; } = [];
    public List<ReportArtifact> Artifacts { get; } = [];
    public Dictionary<string, string> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);
}
