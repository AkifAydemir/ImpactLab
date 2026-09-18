using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Verification;

public static class VerificationSuiteRunner
{
    public static VerificationResult Evaluate(
        VerificationCase @case,
        SimulationResult result,
        IReadOnlyList<ProbeSeries> probes
    )
    {
        @case.Validate();
        var bindings = @case.Bindings.ToDictionary(
            x => x.ReferenceId,
            StringComparer.OrdinalIgnoreCase
        );
        var rows = new List<VerificationSignalResult>();
        foreach (var reference in @case.References)
        {
            if (!bindings.TryGetValue(reference.Id, out var binding))
            {
                rows.Add(
                    new VerificationSignalResult(
                        reference.Id,
                        new VerificationMetricSet(
                            double.PositiveInfinity,
                            double.PositiveInfinity,
                            double.PositiveInfinity,
                            double.PositiveInfinity,
                            0,
                            0
                        ),
                        false
                    )
                );
                continue;
            }
            var candidate = VerificationSignalResolver.Resolve(binding, result, probes);
            var metrics = SignalComparator.Compare(reference, candidate);
            rows.Add(
                new VerificationSignalResult(
                    reference.Id,
                    metrics,
                    metrics.Rmse <= @case.RmseTolerance
                        && metrics.PeakRelativeError <= @case.PeakRelativeTolerance
                )
            );
        }
        return new VerificationResult(@case.Id, @case.Name, rows);
    }
}
