namespace ImpactLab.Core.Experiments.Uncertainty;

public sealed class LatinHypercubeSampler
{
    public IReadOnlyList<IReadOnlyDictionary<string, double>> Generate(
        IReadOnlyList<ParameterDistribution> d,
        int count,
        int seed
    )
    {
        var rng = new Random(seed);
        var bins = d.ToDictionary(
            x => x.ParameterId,
            x => Enumerable.Range(0, count).OrderBy(_ => rng.Next()).ToArray()
        );
        var rows = new List<IReadOnlyDictionary<string, double>>();
        for (var i = 0; i < count; i++)
        {
            var row = new Dictionary<string, double>();
            foreach (var p in d)
            {
                p.Validate();
                var u = (bins[p.ParameterId][i] + rng.NextDouble()) / count;
                var z = Math.Sqrt(2) * InverseErf(2 * u - 1);
                row[p.ParameterId] = DistributionSampler.Sample(p, u, z);
            }
            rows.Add(row);
        }
        return rows;
    }

    private static double InverseErf(double x)
    {
        var a = 0.147;
        var ln = Math.Log(1 - x * x);
        var s = 2 / (Math.PI * a) + ln / 2;
        return Math.Sign(x) * Math.Sqrt(Math.Sqrt(s * s - ln / a) - s);
    }
}
