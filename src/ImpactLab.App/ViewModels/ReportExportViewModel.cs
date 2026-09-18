using ImpactLab.App.Services;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Reporting;

namespace ImpactLab.App.ViewModels;

public sealed class ReportExportViewModel
{
    private readonly ReportExportWorkspaceService _service = new();
    public EngineeringReportDocument? Current { get; private set; }
    public string Status { get; private set; } = "No report";

    public void Build(string scenario, string backend, BackendResultSummary summary)
    {
        Current = _service.Build(scenario, backend, summary);
        Status = "Report ready";
    }

    public void Save(string path)
    {
        if (Current is null)
            throw new InvalidOperationException("Build a report first.");
        _service.Save(path, Current);
        Status = $"Saved {Path.GetFileName(path)}";
    }

    public ReportPackageBuildResult SavePackage(
        string directory,
        ReportGenerationContext context,
        string templateId = "engineering-standard"
    )
    {
        if (Current is null)
            throw new InvalidOperationException("Build a report first.");
        var result = _service.SavePackage(directory, Current, context, templateId);
        Status = $"Package ready: {Path.GetFileName(result.ManifestPath)}";
        return result;
    }
}
