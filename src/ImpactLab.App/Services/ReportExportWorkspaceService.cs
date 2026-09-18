using ImpactLab.Core.Analysis;
using ImpactLab.Core.Reporting;

namespace ImpactLab.App.Services;

public sealed class ReportExportWorkspaceService
{
    private readonly ReportExportService _export = new();

    public EngineeringReportDocument Build(
        string scenario,
        string backend,
        BackendResultSummary summary
    ) => EngineeringReportBuilder.Build(scenario, backend, summary);

    public void Save(string path, EngineeringReportDocument document) =>
        _export.Export(path, document);

    public ReportPackageBuildResult SavePackage(
        string directory,
        EngineeringReportDocument document,
        ReportGenerationContext context,
        string templateId = "engineering-standard"
    ) => _export.ExportPackage(directory, document, context, templateId);
}
