using System.Text;

namespace ImpactLab.Core.Reporting;

public sealed class ReportExportService
{
    public void Export(string path, EngineeringReportDocument document)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        switch (ext)
        {
            case ".html":
            case ".htm":
                HtmlReportExporter.Export(path, document);
                break;
            case ".csv":
                ReportCsvExporter.ExportMetrics(path, document);
                break;
            case ".md":
                File.WriteAllText(
                    path,
                    new MarkdownEngineeringReportExporter().Export(document),
                    new UTF8Encoding(false)
                );
                break;
            case ".json":
                File.WriteAllText(
                    path,
                    EngineeringReportCanonicalJson.Serialize(document),
                    new UTF8Encoding(false)
                );
                break;
            default:
                throw new NotSupportedException($"Unsupported report extension: {ext}");
        }
    }

    public ReportPackageBuildResult ExportPackage(
        string directory,
        EngineeringReportDocument document,
        ReportGenerationContext context,
        string templateId,
        ReportAssetStore? assets = null
    ) =>
        new DeterministicReportPackageBuilder().Build(
            directory,
            document,
            context,
            templateId,
            assets
        );
}
