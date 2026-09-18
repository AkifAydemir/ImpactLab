namespace ImpactLab.Core.Experiments.Uncertainty;

public static class UncertaintyAnalyzer
{
    public static UncertaintySummary Summarize(string metric, IEnumerable<double> values)
    {
        var ordered = values.OrderBy(value => value).ToArray();
        if (ordered.Length == 0)
            throw new ArgumentException("At least one value is required.", nameof(values));

        var mean = ordered.Average();
        var standardDeviation = Math.Sqrt(
            ordered.Sum(value => (value - mean) * (value - mean)) / Math.Max(1, ordered.Length - 1)
        );
        double Quantile(double probability) =>
            ordered[
                (int)
                    Math.Clamp(
                        Math.Round((ordered.Length - 1) * probability),
                        0,
                        ordered.Length - 1
                    )
            ];

        return new(
            metric,
            ordered.Length,
            mean,
            standardDeviation,
            Quantile(.05),
            Quantile(.5),
            Quantile(.95),
            ordered[0],
            ordered[^1]
        );
    }
}
