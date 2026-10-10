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
            "time_s,kinetic_j,rotational_j,elastic_j,max_displacement_m,max_damage,broken_springs,contact_count,max_penetration_m,normal_force_n,tangential_force_n,friction_energy_j,deformable_contact_count,fragment_cluster_count,detached_fragment_mass_kg,secondary_fragment_contact_count,resolved_fragment_count,reduced_order_fragment_count,statistical_fragment_count,coarse_fragment_kinetic_j,coarse_fragment_unresolved_kinetic_j,coarse_fragment_contact_count,coarse_contact_critical_dt_s,coarse_contact_momentum_residual_n,fragment_birth_count,maximum_fragment_generation"
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
                .Append(',')
                .Append(s.FragmentClusterCount)
                .Append(',')
                .Append(Number(s.DetachedFragmentMassKg))
                .Append(',')
                .Append(s.SecondaryFragmentContactCount)
                .Append(',')
                .Append(s.ResolvedFragmentCount)
                .Append(',')
                .Append(s.ReducedOrderFragmentCount)
                .Append(',')
                .Append(s.StatisticalFragmentCount)
                .Append(',')
                .Append(Number(s.CoarseFragmentKineticEnergyJ))
                .Append(',')
                .Append(Number(s.CoarseFragmentUnresolvedKineticEnergyJ))
                .Append(',')
                .Append(s.CoarseFragmentContactCount)
                .Append(',')
                .Append(Number(s.MinimumCoarseContactCriticalTimeStepSeconds))
                .Append(',')
                .Append(Number(s.CoarseContactMomentumResidualN))
                .Append(',')
                .Append(s.FragmentBirthCount)
                .Append(',')
                .Append(s.MaximumFragmentGeneration)
                .AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, sb.ToString());
    }

    private static string Number(double value) =>
        double.IsPositiveInfinity(value)
            ? "inf"
            : value.ToString("R", CultureInfo.InvariantCulture);
}
