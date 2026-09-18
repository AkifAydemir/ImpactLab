using System.Globalization;
using System.Text;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.IO;

public static class TelemetryCsvWriter
{
    public static void Write(string path, SimulationTelemetrySeries series)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "time_s,kinetic_j,rotational_j,elastic_j,max_displacement_m,max_damage,broken_springs,contact_count,max_penetration_m,normal_force_n,tangential_force_n,friction_energy_j,deformable_contact_count"
        );
        foreach (var s in series.Samples)
        {
            sb.Append(s.TimeSeconds.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.KineticEnergyJ.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.RotationalKineticEnergyJ.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.ElasticEnergyJ.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.MaxDisplacementMeters.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.MaxNodeDamage.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.BrokenSpringCount)
                .Append(',')
                .Append(s.ContactCount)
                .Append(',')
                .Append(s.MaxContactPenetrationMeters.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.TotalNormalContactForceN.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.TotalTangentialContactForceN.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.FrictionEnergyJ.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(s.DeformableContactCount)
                .AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, sb.ToString());
    }
}
