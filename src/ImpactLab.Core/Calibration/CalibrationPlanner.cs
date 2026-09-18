namespace ImpactLab.Core.Calibration;

public static class CalibrationPlanner
{
    public static IReadOnlyList<CalibrationCandidate> BuildGrid(
        CalibrationStudyDefinition definition
    )
    {
        definition.Validate();
        var axes = definition
            .Parameters.Select(p =>
                Enumerable
                    .Range(0, p.Samples)
                    .Select(i =>
                        p.Samples == 1
                            ? p.Minimum
                            : p.Minimum + (p.Maximum - p.Minimum) * i / (p.Samples - 1.0)
                    )
                    .ToArray()
            )
            .ToArray();
        var rows = new List<Dictionary<string, double>> { new(StringComparer.OrdinalIgnoreCase) };
        for (var axis = 0; axis < axes.Length; axis++)
        {
            var next = new List<Dictionary<string, double>>();
            foreach (var row in rows)
            foreach (var value in axes[axis])
            {
                var copy = new Dictionary<string, double>(row, StringComparer.OrdinalIgnoreCase)
                {
                    [definition.Parameters[axis].Path] = value,
                };
                next.Add(copy);
                if (next.Count >= definition.MaxCandidates)
                    break;
            }
            rows = next;
            if (rows.Count >= definition.MaxCandidates)
                break;
        }
        return rows.Take(definition.MaxCandidates)
            .Select((x, i) => new CalibrationCandidate(i, x))
            .ToArray();
    }
}
