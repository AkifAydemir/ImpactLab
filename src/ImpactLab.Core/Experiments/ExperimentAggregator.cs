using ImpactLab.Core.Analysis;

namespace ImpactLab.Core.Experiments;

public static class ExperimentAggregator
{
    public static IReadOnlyList<ExperimentRunResult> Rank(
        ExperimentResult result,
        ResultMetricKind metric,
        bool lowerIsBetter
    )
    {
        var successful = result.Runs.Where(x => x.Succeeded && x.Metrics.TryGet(metric, out _));
        return (
            lowerIsBetter
                ? successful.OrderBy(x => x.Metrics[metric])
                : successful.OrderByDescending(x => x.Metrics[metric])
        ).ToArray();
    }

    public static (double Min, double Max, double Mean) Statistics(
        ExperimentResult result,
        ResultMetricKind metric
    )
    {
        var values = result
            .Runs.Where(x => x.Succeeded && x.Metrics.TryGet(metric, out _))
            .Select(x => x.Metrics[metric])
            .ToArray();
        return values.Length == 0 ? (0, 0, 0) : (values.Min(), values.Max(), values.Average());
    }
}
