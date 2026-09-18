using System.Globalization;
using System.Net;
using System.Text;

namespace ImpactLab.Core.Reporting;

public static class HtmlReportExporter
{
    public static string Render(EngineeringReportDocument doc)
    {
        var b = new StringBuilder();
        b.Append("<!doctype html><html><head><meta charset=\"utf-8\"><style>");
        b.Append(
            "body{font-family:Segoe UI,Arial;margin:36px;color:#1f2937}table{border-collapse:collapse;width:100%}"
        );
        b.Append(
            "td,th{border:1px solid #d1d5db;padding:6px}h1,h2{color:#111827}.metric{margin:4px 0}"
        );
        b.Append("</style></head><body>");
        b.Append(
            CultureInfo.InvariantCulture,
            $"<h1>{E(doc.Title)}</h1><p><b>Scenario:</b> {E(doc.ScenarioName)}<br><b>Backend:</b> {E(doc.Backend)}<br><b>Generated:</b> {E(doc.GeneratedUtc)}</p>"
        );
        foreach (var s in doc.Sections)
        {
            b.Append(CultureInfo.InvariantCulture, $"<h2>{E(s.Title)}</h2>");
            if (!string.IsNullOrWhiteSpace(s.Narrative))
                b.Append(CultureInfo.InvariantCulture, $"<p>{E(s.Narrative)}</p>");
            foreach (var m in s.Metrics)
                b.Append(
                    CultureInfo.InvariantCulture,
                    $"<div class=\"metric\"><b>{E(m.Name)}:</b> {E(m.Value)} {E(m.Unit ?? string.Empty)}</div>"
                );
            foreach (var t in s.Tables)
            {
                b.Append(CultureInfo.InvariantCulture, $"<h3>{E(t.Title)}</h3><table><thead><tr>");
                foreach (var c in t.Columns)
                    b.Append(CultureInfo.InvariantCulture, $"<th>{E(c)}</th>");
                b.Append("</tr></thead><tbody>");
                foreach (var row in t.Rows)
                {
                    b.Append("<tr>");
                    foreach (var c in row)
                        b.Append(CultureInfo.InvariantCulture, $"<td>{E(c)}</td>");
                    b.Append("</tr>");
                }
                b.Append("</tbody></table>");
            }
        }
        return b.Append("</body></html>\n").ToString();
    }

    public static void Export(string path, EngineeringReportDocument doc) =>
        File.WriteAllText(path, Render(doc), new UTF8Encoding(false));

    private static string E(string value) => WebUtility.HtmlEncode(value);
}
