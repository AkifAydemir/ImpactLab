using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class EnergyBalanceAnalyzer
{
    public static EnergyBalanceSeries Analyze(SimulationResult result)
    {
        if (result.Telemetry.Samples.Count == 0)
            return new(0.0, []);
        var first = result.Telemetry.Samples[0];
        var reference =
            first.KineticEnergyJ + first.RotationalKineticEnergyJ + first.ElasticEnergyJ;
        var samples = result
            .Telemetry.Samples.Select(s =>
            {
                var mechanical = s.KineticEnergyJ + s.RotationalKineticEnergyJ + s.ElasticEnergyJ;
                var residual = mechanical + s.FrictionEnergyJ - reference;
                var pct = Math.Abs(reference) <= 1e-12 ? 0.0 : 100.0 * residual / reference;
                return new EnergyBalanceSample(
                    s.TimeSeconds,
                    mechanical,
                    s.FrictionEnergyJ,
                    residual,
                    pct
                );
            })
            .ToArray();
        return new(reference, samples);
    }
}
