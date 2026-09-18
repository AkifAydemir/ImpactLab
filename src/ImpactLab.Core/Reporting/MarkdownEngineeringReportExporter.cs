using System.Globalization;
using System.Text;

namespace ImpactLab.Core.Reporting;

public sealed class MarkdownEngineeringReportExporter
{
    public string Export(EngineeringReportDocument document)
    {
        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"# {document.Title}").AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"**Scenario:** {document.ScenarioName} ");
        builder.AppendLine(CultureInfo.InvariantCulture, $"**Backend:** {document.Backend} ");
        builder
            .AppendLine(CultureInfo.InvariantCulture, $"**Generated:** {document.GeneratedUtc}")
            .AppendLine();
        foreach (var section in document.Sections)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"## {section.Title}").AppendLine();
            if (!string.IsNullOrWhiteSpace(section.Narrative))
                builder.AppendLine(section.Narrative).AppendLine();
            foreach (var metric in section.Metrics)
            {
                var unit = string.IsNullOrWhiteSpace(metric.Unit)
                    ? string.Empty
                    : $" {metric.Unit}";
                builder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"- **{metric.Name}:** {metric.Value}{unit}"
                );
            }
            if (section.Metrics.Count > 0)
                builder.AppendLine();
            foreach (var table in section.Tables)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"### {table.Title}").AppendLine();
                builder.AppendLine("| " + string.Join(" | ", table.Columns.Select(Escape)) + " |");
                builder.AppendLine(
                    "| " + string.Join(" | ", table.Columns.Select(_ => "---")) + " |"
                );
                foreach (var row in table.Rows)
                    builder.AppendLine("| " + string.Join(" | ", row.Select(Escape)) + " |");
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
