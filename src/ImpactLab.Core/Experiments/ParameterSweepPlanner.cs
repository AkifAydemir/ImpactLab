namespace ImpactLab.Core.Experiments;

public static class ParameterSweepPlanner
{
    public static IReadOnlyList<SweepPoint> Expand(SweepDefinition definition)
    {
        definition.Validate();
        var rows = new List<Dictionary<string, double>> { new(StringComparer.OrdinalIgnoreCase) };
        foreach (var axis in definition.Axes)
        {
            var next = new List<Dictionary<string, double>>();
            foreach (var row in rows)
            foreach (var value in axis.Values)
            {
                var clone = new Dictionary<string, double>(row, StringComparer.OrdinalIgnoreCase)
                {
                    [axis.Path] = value,
                };
                next.Add(clone);
                if (next.Count > definition.MaxRuns)
                    throw new InvalidOperationException(
                        $"Sweep exceeds MaxRuns ({definition.MaxRuns})."
                    );
            }
            rows = next;
        }
        return rows.Select((x, i) => new SweepPoint(i, x)).ToArray();
    }
}
