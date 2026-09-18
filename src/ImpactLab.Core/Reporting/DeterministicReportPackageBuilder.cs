using System.Text;
using System.Text.Json;

namespace ImpactLab.Core.Reporting;

public sealed class DeterministicReportPackageBuilder
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public ReportPackageBuildResult Build(
        string outputDirectory,
        EngineeringReportDocument document,
        ReportGenerationContext context,
        string templateId,
        ReportAssetStore? assets = null
    )
    {
        Directory.CreateDirectory(outputDirectory);
        var reportId = StableReportId(context, templateId);
        var files = new SortedDictionary<string, (string MediaType, byte[] Content)>(
            StringComparer.Ordinal
        )
        {
            ["report.md"] = (
                "text/markdown",
                Utf8(new MarkdownEngineeringReportExporter().Export(document))
            ),
            ["report.html"] = ("text/html", Utf8(HtmlReportExporter.Render(document))),
            ["metrics.csv"] = ("text/csv", Utf8(ReportCsvExporter.RenderMetrics(document))),
            ["report.json"] = (
                "application/json",
                Utf8(EngineeringReportCanonicalJson.Serialize(document))
            ),
        };
        if (assets is not null)
            foreach (var name in assets.Names)
                if (assets.TryGet(name, out var content))
                    files[$"assets/{SanitizeRelative(name)}"] = (
                        "application/octet-stream",
                        content.ToArray()
                    );
        var entries = new List<ReportPackageFileEntry>();
        foreach (var pair in files)
        {
            var path = Path.Combine(
                outputDirectory,
                pair.Key.Replace('/', Path.DirectorySeparatorChar)
            );
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, pair.Value.Content);
            entries.Add(
                new(
                    pair.Key,
                    pair.Value.MediaType,
                    pair.Value.Content.LongLength,
                    ReportPackageHash.Bytes(pair.Value.Content)
                )
            );
        }
        var manifest = new DeterministicReportPackageManifest(
            1,
            reportId,
            templateId,
            context.ScenarioFingerprint,
            context.BackendId,
            context.SoftwareVersion,
            context.GeneratedUtc,
            entries
        );
        var manifestPath = Path.Combine(outputDirectory, "manifest.json");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(manifest, Json) + "\n",
            new UTF8Encoding(false)
        );
        return new(outputDirectory, manifest, manifestPath);
    }

    private static string StableReportId(ReportGenerationContext context, string templateId)
    {
        var raw =
            $"{context.ScenarioFingerprint}|{context.BackendId}|{templateId}|{context.GeneratedUtc:O}";
        return ReportPackageHash.Bytes(Encoding.UTF8.GetBytes(raw))[..24];
    }

    private static byte[] Utf8(string value) => new UTF8Encoding(false).GetBytes(value);

    private static string SanitizeRelative(string value) =>
        string.Join(
            '/',
            value
                .Replace('\\', '/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part =>
                    string.Concat(
                        part.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)
                    )
                )
        );
}
