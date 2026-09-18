using ImpactLab.Core.Constraints;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Verification;

public static class VerificationSignalResolver
{
    public static IReadOnlyList<ReferenceSignalPoint> Resolve(
        SignalBinding binding,
        SimulationResult result,
        IReadOnlyList<ProbeSeries> probes
    )
    {
        binding.Validate();
        IEnumerable<(double Time, double Value)> raw = binding.Kind switch
        {
            SignalBindingKind.ProbeAverage => Probe(binding, probes)
                .Samples.Select(sample => (sample.TimeSeconds, sample.Average)),
            SignalBindingKind.ProbeMaximum => Probe(binding, probes)
                .Samples.Select(sample => (sample.TimeSeconds, sample.Maximum)),
            SignalBindingKind.ProbeMinimum => Probe(binding, probes)
                .Samples.Select(sample => (sample.TimeSeconds, sample.Minimum)),
            SignalBindingKind.TelemetryKineticEnergy => result.Telemetry.Samples.Select(sample =>
                (sample.TimeSeconds, sample.KineticEnergyJ + sample.RotationalKineticEnergyJ)
            ),
            SignalBindingKind.TelemetryElasticEnergy => result.Telemetry.Samples.Select(sample =>
                (sample.TimeSeconds, sample.ElasticEnergyJ)
            ),
            SignalBindingKind.TelemetryContactForce => result.Telemetry.Samples.Select(sample =>
                (sample.TimeSeconds, sample.TotalNormalContactForceN)
            ),
            SignalBindingKind.TelemetryDisplacement => result.Telemetry.Samples.Select(sample =>
                (sample.TimeSeconds, sample.MaxDisplacementMeters)
            ),
            SignalBindingKind.ConstraintReactionMagnitude => Reaction(
                    binding,
                    result.ConstraintReactions
                )
                .Select(sample => (sample.TimeSeconds, sample.MagnitudeN)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(binding),
                binding.Kind,
                "Unsupported signal binding kind."
            ),
        };
        return raw.Select(sample => new ReferenceSignalPoint(
                sample.Time,
                sample.Value * binding.Scale + binding.Offset
            ))
            .ToArray();
    }

    private static ProbeSeries Probe(SignalBinding binding, IReadOnlyList<ProbeSeries> probes) =>
        probes.First(series =>
            string.Equals(
                series.Definition.Id,
                binding.SourceId,
                StringComparison.OrdinalIgnoreCase
            )
        );

    private static IReadOnlyList<ConstraintReactionSample> Reaction(
        SignalBinding binding,
        ConstraintReactionSeries series
    ) => series.ByConstraint.TryGetValue(binding.SourceId!, out var values) ? values : [];
}
