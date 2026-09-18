namespace ImpactLab.Core.Experiments.Sensitivity;

public static class SobolBootstrap
{
    public static SensitivityConfidenceInterval Estimate(
        IReadOnlyList<double> values,
        int samples = 500,
        int seed = 1
    )
    {
        if (values.Count == 0)
            return new(0, 0, 0, samples);
        var rng = new Random(seed);
        var means = new double[samples];
        for (var s = 0; s < samples; s++)
        {
            double m = 0;
            for (var i = 0; i < values.Count; i++)
                m += values[rng.Next(values.Count)];
            means[s] = m / values.Count;
        }
        Array.Sort(means);
        return new(
            values.Average(),
            means[(int)(0.025 * (samples - 1))],
            means[(int)(0.975 * (samples - 1))],
            samples
        );
    }
}
