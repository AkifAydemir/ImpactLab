namespace ImpactLab.Core.Verification;

public static class SignalComparator
{
    public static VerificationMetricSet Compare(
        ReferenceSignal reference,
        IReadOnlyList<ReferenceSignalPoint> candidate
    )
    {
        reference.Validate();
        if (candidate.Count < 2)
            throw new InvalidOperationException("Candidate signal requires at least two points.");
        var pairs = reference
            .Points.Select(p =>
                (Reference: p.Value, Candidate: Interpolate(candidate, p.TimeSeconds))
            )
            .ToArray();
        var errors = pairs.Select(x => x.Candidate - x.Reference).ToArray();
        var rmse = Math.Sqrt(errors.Average(x => x * x));
        var mae = errors.Average(Math.Abs);
        var peak = errors.Max(Math.Abs);
        var scale = Math.Max(reference.Points.Max(x => Math.Abs(x.Value)), 1e-12);
        var corr = Correlation(
            pairs.Select(x => x.Reference).ToArray(),
            pairs.Select(x => x.Candidate).ToArray()
        );
        return new VerificationMetricSet(rmse, mae, peak, peak / scale, corr, pairs.Length);
    }

    private static double Interpolate(IReadOnlyList<ReferenceSignalPoint> points, double time)
    {
        if (time <= points[0].TimeSeconds)
            return points[0].Value;
        if (time >= points[^1].TimeSeconds)
            return points[^1].Value;
        for (var i = 1; i < points.Count; i++)
            if (points[i].TimeSeconds >= time)
            {
                var a = points[i - 1];
                var b = points[i];
                var t = (time - a.TimeSeconds) / (b.TimeSeconds - a.TimeSeconds);
                return a.Value + (b.Value - a.Value) * t;
            }
        return points[^1].Value;
    }

    private static double Correlation(double[] a, double[] b)
    {
        var ma = a.Average();
        var mb = b.Average();
        var num = a.Zip(b).Sum(x => (x.First - ma) * (x.Second - mb));
        var da = Math.Sqrt(a.Sum(x => Math.Pow(x - ma, 2)));
        var db = Math.Sqrt(b.Sum(x => Math.Pow(x - mb, 2)));
        return da <= 1e-18 || db <= 1e-18 ? 0.0 : num / (da * db);
    }
}
