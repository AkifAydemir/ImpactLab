using System.IO.Compression;
using System.Text;

namespace ImpactLab.Core.Reporting;

public sealed class ReportPackageExporter
{
    public void Export(string path, EngineeringReportDocument doc, ReportAssetStore assets)
    {
        using var fs = File.Create(path);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
        var report = zip.CreateEntry("report.md");
        using (var w = new StreamWriter(report.Open(), Encoding.UTF8))
            w.Write(new MarkdownEngineeringReportExporter().Export(doc));
        foreach (var n in assets.Names)
        {
            var e = zip.CreateEntry("assets/" + n);
            using var s = e.Open();
            s.Write(assets.TryGet(n, out var b) ? b : []);
        }
    }
}
