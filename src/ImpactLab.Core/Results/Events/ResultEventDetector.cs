namespace ImpactLab.Core.Results.Events;

public static class ResultEventDetector
{
    public static IReadOnlyList<ResultEvent> Detect(
        IReadOnlyList<double[]> frames,
        IReadOnlyList<double> times,
        ThresholdEventRule rule
    )
    {
        var list = new List<ResultEvent>();
        for (var i = 0; i < frames.Count; i++)
        {
            var peak = frames[i].Length == 0 ? 0 : frames[i].Max();
            var hit = rule.GreaterThan ? peak >= rule.Threshold : peak <= rule.Threshold;
            if (hit)
                list.Add(
                    new(
                        $"{rule.Id}:{i}",
                        rule.Id,
                        times[Math.Min(i, times.Count - 1)],
                        i,
                        rule.Severity,
                        $"{rule.FieldId} threshold crossed.",
                        new Dictionary<string, double>
                        {
                            { "peak", peak },
                            { "threshold", rule.Threshold },
                        }
                    )
                );
        }
        return list;
    }
}
