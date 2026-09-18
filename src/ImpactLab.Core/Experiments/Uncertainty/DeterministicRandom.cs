namespace ImpactLab.Core.Experiments.Uncertainty;

public sealed class DeterministicRandom(int seed)
{
    private readonly Random _r = new(seed);

    public double NextOpen() => Math.Clamp(_r.NextDouble(), 1e-12, 1 - 1e-12);

    public double Normal()
    {
        var u1 = NextOpen();
        var u2 = NextOpen();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    }
}
