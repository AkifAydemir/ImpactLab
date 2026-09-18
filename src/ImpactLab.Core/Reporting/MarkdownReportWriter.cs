using System.Globalization;
using System.Text;

namespace ImpactLab.Core.Reporting;

public static class MarkdownReportWriter
{
    public static string Write(EngineeringReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"# {report.Title}");
        if (!string.IsNullOrWhiteSpace(report.Subtitle))
            sb.AppendLine(CultureInfo.InvariantCulture, $"\n_{report.Subtitle}_");
        sb.AppendLine(CultureInfo.InvariantCulture, $"\nGenerated: {report.CreatedAt:O}\n");
        foreach (var section in report.Sections)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"## {section.Title}\n");
            foreach (var paragraph in section.Paragraphs)
                sb.AppendLine(paragraph + "\n");
            foreach (var table in section.Tables)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"### {table.Title}\n");
                sb.AppendLine("| " + string.Join(" | ", table.Columns) + " |");
                sb.AppendLine("| " + string.Join(" | ", table.Columns.Select(_ => "---")) + " |");
                foreach (var row in table.Rows)
                    sb.AppendLine("| " + string.Join(" | ", row.Select(Escape)) + " |");
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    public static void Save(string path, EngineeringReport report) =>
        File.WriteAllText(path, Write(report), Encoding.UTF8);

    private static string Escape(string value) =>
        value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
