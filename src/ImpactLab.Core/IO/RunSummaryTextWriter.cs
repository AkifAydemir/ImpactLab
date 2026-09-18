using System.Globalization;
using System.Text;
using ImpactLab.Core.Analysis;

namespace ImpactLab.Core.IO;

public static class RunSummaryTextWriter
{
    public static void Write(string path, RunResultSummary summary)
    {
        var c = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("ImpactLab Run Summary");
        sb.AppendLine(c, $"End time [s]: {summary.EndTimeSeconds:G9}");
        sb.AppendLine(c, $"Peak kinetic energy [J]: {summary.PeakKineticEnergyJ:G9}");
        sb.AppendLine(c, $"Peak elastic energy [J]: {summary.PeakElasticEnergyJ:G9}");
        sb.AppendLine(c, $"Max displacement [m]: {summary.MaxDisplacementMeters:G9}");
        sb.AppendLine(c, $"Broken springs: {summary.BrokenSpringCount}");
        sb.AppendLine(c, $"Peak normal contact force [N]: {summary.PeakContactForceN:G9}");
        sb.AppendLine(c, $"Peak tangential contact force [N]: {summary.PeakTangentialForceN:G9}");
        sb.AppendLine(c, $"Friction energy [J]: {summary.FrictionEnergyJ:G9}");
        sb.AppendLine();
        sb.AppendLine("Parts");
        foreach (var part in summary.Parts)
            sb.AppendLine(
                c,
                $"- {part.PartId}: nodes={part.NodeCount}, springs={part.SpringCount}, broken={part.BrokenSpringCount}, maxDisp={part.MaxDisplacementMeters:G9}"
            );
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, sb.ToString());
    }
}
