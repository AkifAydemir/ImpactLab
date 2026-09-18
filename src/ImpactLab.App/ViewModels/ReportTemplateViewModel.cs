using ImpactLab.Core.Reporting.Templates;

namespace ImpactLab.App.ViewModels;

public sealed class ReportTemplateViewModel
{
    public IReadOnlyList<ReportTemplate> Templates => ReportTemplateCatalog.BuiltIns;
    public ReportTemplate Selected { get; set; } = ReportTemplateCatalog.BuiltIns[0];
}
