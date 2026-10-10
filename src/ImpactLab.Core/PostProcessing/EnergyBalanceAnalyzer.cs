using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class EnergyBalanceAnalyzer
{
    public static EnergyBalanceSeries Analyze(SimulationResult result)
    {
        if (result.Telemetry.Samples.Count == 0)
            return new(0.0, []);
        var first = result.Telemetry.Samples[0];
        var firstMechanical =
            first.KineticEnergyJ
            + first.RotationalKineticEnergyJ
            + first.ElasticEnergyJ
            + first.ContactPotentialEnergyJ;
        var firstDissipated =
            first.FrictionEnergyJ
            + first.ContactDampingEnergyJ
            + first.StructuralDampingEnergyJ
            + first.GlobalDampingEnergyJ
            + first.PlasticDissipationEnergyJ
            + first.FractureDissipationEnergyJ
            + first.ThermalEnergyJ;
        var reference = firstMechanical + firstDissipated;
        var samples = result
            .Telemetry.Samples.Select(s =>
            {
                var mechanical =
                    s.KineticEnergyJ
                    + s.RotationalKineticEnergyJ
                    + s.ElasticEnergyJ
                    + s.ContactPotentialEnergyJ;
                var dissipated =
                    s.FrictionEnergyJ
                    + s.ContactDampingEnergyJ
                    + s.StructuralDampingEnergyJ
                    + s.GlobalDampingEnergyJ
                    + s.PlasticDissipationEnergyJ
                    + s.FractureDissipationEnergyJ
                    + s.ThermalEnergyJ;
                var accounted = mechanical + dissipated;
                var expected =
                    reference
                    + (s.ExternalWorkJ - first.ExternalWorkJ)
                    + (s.ConstraintWorkJ - first.ConstraintWorkJ);
                var residual = accounted - expected;
                var pct = Math.Abs(reference) <= 1e-12 ? 0.0 : 100.0 * residual / reference;
                return new EnergyBalanceSample(
                    s.TimeSeconds,
                    mechanical,
                    s.FrictionEnergyJ,
                    residual,
                    pct,
                    s.ContactPotentialEnergyJ,
                    s.ContactDampingEnergyJ,
                    s.PlasticDissipationEnergyJ,
                    s.FractureDissipationEnergyJ,
                    s.ThermalEnergyJ,
                    s.ExternalWorkJ,
                    s.ConstraintWorkJ,
                    accounted,
                    s.StructuralDampingEnergyJ,
                    s.GlobalDampingEnergyJ
                );
            })
            .ToArray();
        return new(reference, samples);
    }
}
