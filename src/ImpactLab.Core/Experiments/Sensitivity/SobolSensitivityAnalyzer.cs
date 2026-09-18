namespace ImpactLab.Core.Experiments.Sensitivity;

public sealed class SobolSensitivityAnalyzer
{
    public IReadOnlyList<SensitivityIndex> Analyze(
        IReadOnlyList<string> names,
        double[,] a,
        double[,] b,
        double[] ya,
        double[] yb,
        Func<int, double[]> mixedEvaluator
    )
    {
        var v = Variance(ya.Concat(yb));
        var result = new List<SensitivityIndex>();
        for (var j = 0; j < names.Count; j++)
        {
            var yc = mixedEvaluator(j);
            double first = 0,
                total = 0;
            for (var i = 0; i < ya.Length; i++)
            {
                first += yb[i] * (yc[i] - ya[i]);
                var d = ya[i] - yc[i];
                total += d * d * 0.5;
            }
            first /= Math.Max(ya.Length * v, 1e-30);
            total /= Math.Max(ya.Length * v, 1e-30);
            result.Add(new(names[j], first, total, first, total));
        }
        return result;
    }

    private static double Variance(IEnumerable<double> x)
    {
        var a = x.ToArray();
        var m = a.Average();
        return a.Select(v => (v - m) * (v - m)).Average();
    }
}
