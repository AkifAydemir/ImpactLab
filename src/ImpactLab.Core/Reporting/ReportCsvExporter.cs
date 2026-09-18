using System.Globalization;
using System.Text;

namespace ImpactLab.Core.Reporting;

public static class ReportCsvExporter
{
    public static string RenderMetrics(EngineeringReportDocument doc)
    {
        var b = new StringBuilder("Section,Metric,Value,Unit\n");
        foreach (var section in doc.Sections)
        foreach (var metric in section.Metrics)
            b.AppendLine(
                CultureInfo.InvariantCulture,
                $"{Q(section.Title)},{Q(metric.Name)},{Q(metric.Value)},{Q(metric.Unit ?? string.Empty)}"
            );
        return b.ToString();
    }

    public static void ExportMetrics(string path, EngineeringReportDocument doc) =>
        File.WriteAllText(path, RenderMetrics(doc), new UTF8Encoding(false));

    private static string Q(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
