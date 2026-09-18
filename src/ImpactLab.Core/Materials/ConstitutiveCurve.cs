namespace ImpactLab.Core.Materials;

public sealed class ConstitutiveCurve
{
    private readonly ConstitutiveCurvePoint[] _points;

    public ConstitutiveCurve(IEnumerable<ConstitutiveCurvePoint> points)
    {
        _points = points.OrderBy(x => x.Strain).ToArray();
        if (_points.Length < 2)
            throw new ArgumentException("At least two constitutive points are required.");
        foreach (var p in _points)
            p.Validate();
        if (_points.Zip(_points.Skip(1)).Any(x => x.First.Strain >= x.Second.Strain))
            throw new InvalidOperationException(
                "Constitutive strain values must be strictly increasing."
            );
    }

    public IReadOnlyList<ConstitutiveCurvePoint> Points => _points;

    public double EvaluateStress(double strain)
    {
        if (strain <= _points[0].Strain)
            return _points[0].StressPa;
        if (strain >= _points[^1].Strain)
            return _points[^1].StressPa;
        var hi = Array.FindIndex(_points, x => x.Strain >= strain);
        var a = _points[hi - 1];
        var b = _points[hi];
        var t = (strain - a.Strain) / (b.Strain - a.Strain);
        return a.StressPa + (b.StressPa - a.StressPa) * t;
    }

    public double SecantModulus(double strain)
    {
        if (Math.Abs(strain) <= 1e-15)
            return 0.0;
        return EvaluateStress(strain) / strain;
    }
}
