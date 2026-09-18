namespace ImpactLab.Core.Continuum.Nonlinear;

public static class ResidualNormEvaluator
{
    public static double L2(ReadOnlySpan<double> x)
    {
        double s = 0;
        for (var i = 0; i < x.Length; i++)
            s += x[i] * x[i];
        return Math.Sqrt(s);
    }

    public static double Relative(ReadOnlySpan<double> x, double reference) =>
        L2(x) / Math.Max(Math.Abs(reference), 1e-30);
}
