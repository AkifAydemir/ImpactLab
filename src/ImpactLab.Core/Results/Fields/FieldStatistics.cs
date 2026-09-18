namespace ImpactLab.Core.Results.Fields;

public static class FieldStatistics
{
    public static FieldRange Compute(ReadOnlySpan<double> x)
    {
        if (x.Length == 0)
            return default;
        double min = double.PositiveInfinity,
            max = double.NegativeInfinity,
            sum = 0,
            sq = 0;
        foreach (var v in x)
        {
            min = Math.Min(min, v);
            max = Math.Max(max, v);
            sum += v;
            sq += v * v;
        }
        return new(min, max, sum / x.Length, Math.Sqrt(sq / x.Length));
    }
}
