namespace ImpactLab.Core.Experiments.Optimization;

public sealed class GaussianProcessSurrogate
{
    private readonly GaussianProcessKernel _k;
    private double[][] _x = [];
    private double[] _y = [];

    public GaussianProcessSurrogate(GaussianProcessKernel k) => _k = k;

    public void Fit(IReadOnlyList<double[]> x, IReadOnlyList<double> y)
    {
        _x = x.Select(a => a.ToArray()).ToArray();
        _y = y.ToArray();
    }

    public (double Mean, double Variance) Predict(ReadOnlySpan<double> x)
    {
        if (_x.Length == 0)
            return (0, _k.SignalVariance);
        double w = 0,
            m = 0;
        for (var i = 0; i < _x.Length; i++)
        {
            var k = _k.Evaluate(x, _x[i]);
            w += k;
            m += k * _y[i];
        }
        return (w <= 1e-30 ? 0 : m / w, Math.Max(_k.NoiseVariance, _k.SignalVariance / (1 + w)));
    }
}
