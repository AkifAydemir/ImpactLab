using System.Globalization;
using System.Security;
using System.Text;

namespace ImpactLab.Core.Visualization;

public static class SvgChartExporter
{
    public static string Export(ChartDocument document, int width = 1200, int height = 700)
    {
        var view = ChartViewport.Auto(document);
        var sb = new StringBuilder();
        sb.Append(
            CultureInfo.InvariantCulture,
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>"
        );
        sb.Append("<rect width='100%' height='100%' fill='white'/>");
        sb.Append(
            CultureInfo.InvariantCulture,
            $"<text x='60' y='34' font-size='22' font-family='sans-serif'>{SecurityElement.Escape(document.Title)}</text>"
        );
        var palette = new[] { "#1565c0", "#c62828", "#2e7d32", "#6a1b9a", "#ef6c00", "#00838f" };
        for (var s = 0; s < document.Series.Count; s++)
        {
            var series = document.Series[s];
            if (series.Points.Count < 2)
                continue;
            var pts = string.Join(
                " ",
                series.Points.Select(p => FormattableString.Invariant($"{X(p.X):F2},{Y(p.Y):F2}"))
            );
            sb.Append(
                CultureInfo.InvariantCulture,
                $"<polyline fill='none' stroke='{palette[s % palette.Length]}' stroke-width='2' points='{pts}'/>"
            );
        }
        sb.Append("</svg>");
        return sb.ToString();
        double X(double x) => 60 + (x - view.XMin) / view.XSpan * (width - 90);
        double Y(double y) => height - 50 - (y - view.YMin) / view.YSpan * (height - 100);
    }

    public static void Save(
        string path,
        ChartDocument document,
        int width = 1200,
        int height = 700
    ) => File.WriteAllText(path, Export(document, width, height));
}
