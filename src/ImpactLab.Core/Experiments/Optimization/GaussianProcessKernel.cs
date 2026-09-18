namespace ImpactLab.Core.Experiments.Optimization;

public sealed record GaussianProcessKernel(
    double LengthScale = 1,
    double SignalVariance = 1,
    double NoiseVariance = 1e-8
)
{
    public double Evaluate(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        double s = 0;
        for (var i = 0; i < a.Length; i++)
        {
            var d = (a[i] - b[i]) / Math.Max(LengthScale, 1e-12);
            s += d * d;
        }
        return SignalVariance * Math.Exp(-0.5 * s);
    }
}
