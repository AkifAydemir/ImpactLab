using System.IO.Compression;
using System.Text.Json;

namespace ImpactLab.Core.Reporting;

public static class ReportPackageWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static void WriteZip(string path, EngineeringReportPackage package)
    {
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        var meta = zip.CreateEntry("manifest.json");
        using (var w = new StreamWriter(meta.Open()))
            w.Write(JsonSerializer.Serialize(package.Metadata, SerializerOptions));
        foreach (var a in package.Artifacts)
        {
            var e = zip.CreateEntry("artifacts/" + Sanitize(a.Name));
            using var s = e.Open();
            s.Write(a.Content);
        }
        var md = zip.CreateEntry("report.md");
        using (var w = new StreamWriter(md.Open()))
            w.Write(MarkdownReportWriter.Write(package.Report));
    }

    private static string Sanitize(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
