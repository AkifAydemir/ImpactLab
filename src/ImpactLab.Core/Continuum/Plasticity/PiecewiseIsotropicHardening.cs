namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class PiecewiseIsotropicHardening : IHardeningLaw
{
    private readonly PiecewiseHardeningPoint[] _p;

    public PiecewiseIsotropicHardening(IEnumerable<PiecewiseHardeningPoint> points)
    {
        _p = points.OrderBy(x => x.PlasticStrain).ToArray();
        if (_p.Length < 2)
            throw new ArgumentException("At least two hardening points required.");
    }

    public double YieldStress(double e)
    {
        if (e <= _p[0].PlasticStrain)
            return _p[0].YieldStressPa;
        for (var i = 1; i < _p.Length; i++)
            if (e <= _p[i].PlasticStrain)
            {
                var a = _p[i - 1];
                var b = _p[i];
                var t = (e - a.PlasticStrain) / (b.PlasticStrain - a.PlasticStrain);
                return a.YieldStressPa + (b.YieldStressPa - a.YieldStressPa) * t;
            }
        return _p[^1].YieldStressPa;
    }

    public double Tangent(double e)
    {
        for (var i = 1; i < _p.Length; i++)
            if (e <= _p[i].PlasticStrain)
                return (_p[i].YieldStressPa - _p[i - 1].YieldStressPa)
                    / Math.Max(1e-30, _p[i].PlasticStrain - _p[i - 1].PlasticStrain);
        return 0;
    }
}
