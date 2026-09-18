namespace ImpactLab.Core.Experiments.Optimization;

public static class ExpectedImprovement
{
    public static double Evaluate(
        double mean,
        double variance,
        double best,
        double exploration = 0.01
    )
    {
        var s = Math.Sqrt(Math.Max(variance, 1e-30));
        var improvement = best - mean - exploration;
        var z = improvement / s;
        return improvement * NormalCdf(z) + s * NormalPdf(z);
    }

    private static double NormalPdf(double x) => Math.Exp(-0.5 * x * x) / Math.Sqrt(2 * Math.PI);

    private static double NormalCdf(double x) => 0.5 * (1 + Erf(x / Math.Sqrt(2)));

    private static double Erf(double x)
    {
        var sign = Math.Sign(x);
        x = Math.Abs(x);
        var t = 1 / (1 + 0.3275911 * x);
        var y =
            1
            - (
                ((((1.061405429 * t - 1.453152027) * t) + 1.421413741) * t - 0.284496736) * t
                + 0.254829592
            )
                * t
                * Math.Exp(-x * x);
        return sign * y;
    }
}
