namespace ImpactLab.Core.Reporting.Templates;

public sealed class ReportTemplateRenderer
{
    public EngineeringReportDocument Render(ReportTemplate t, EngineeringReportDocument source)
    {
        var wanted = t
            .Sections.Where(x => x.Enabled)
            .Select(x => x.Heading)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sections = source
            .Sections.Where(s =>
                wanted.Count == 0
                || wanted.Contains(s.Title)
                || t.Sections.Any(x => x.Enabled && x.Kind == ReportTemplateSectionKind.CustomText)
            )
            .ToList();
        return source with { Title = t.Title, Sections = sections };
    }
}
