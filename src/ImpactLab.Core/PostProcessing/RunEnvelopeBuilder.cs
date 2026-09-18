namespace ImpactLab.Core.PostProcessing;

public static class RunEnvelopeBuilder
{
    public static RunEnvelope Build(
        string name,
        IReadOnlyList<IReadOnlyList<(double Time, double Value)>> series
    )
    {
        if (series.Count == 0)
            return new RunEnvelope(name, []);
        var times = series
            .SelectMany(x => x.Select(p => p.Time))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        var points = new List<RunEnvelopePoint>(times.Length);
        foreach (var time in times)
        {
            var values = series.Where(x => x.Count > 0).Select(x => Interpolate(x, time)).ToArray();
            if (values.Length > 0)
                points.Add(
                    new RunEnvelopePoint(time, values.Min(), values.Max(), values.Average())
                );
        }
        return new RunEnvelope(name, points);
    }

    private static double Interpolate(IReadOnlyList<(double Time, double Value)> s, double t)
    {
        if (t <= s[0].Time)
            return s[0].Value;
        if (t >= s[^1].Time)
            return s[^1].Value;
        for (var i = 1; i < s.Count; i++)
            if (s[i].Time >= t)
            {
                var a = s[i - 1];
                var b = s[i];
                var q = (t - a.Time) / (b.Time - a.Time);
                return a.Value + (b.Value - a.Value) * q;
            }
        return s[^1].Value;
    }
}
