namespace ImpactLab.Core.Loads;

public sealed class PiecewiseLinearCurve
{
    private readonly LoadCurvePoint[] _points;

    public PiecewiseLinearCurve(IEnumerable<LoadCurvePoint> points)
    {
        _points = points.OrderBy(x => x.TimeSeconds).ToArray();
        if (_points.Length == 0)
            throw new ArgumentException("Curve requires at least one point.", nameof(points));
        if (_points.Any(x => x.TimeSeconds < 0.0))
            throw new ArgumentException("Curve time cannot be negative.", nameof(points));
    }

    public IReadOnlyList<LoadCurvePoint> Points => _points;

    public double Evaluate(double timeSeconds)
    {
        if (timeSeconds <= _points[0].TimeSeconds)
            return _points[0].Value;
        if (timeSeconds >= _points[^1].TimeSeconds)
            return _points[^1].Value;
        var lo = 0;
        var hi = _points.Length - 1;
        while (hi - lo > 1)
        {
            var mid = (lo + hi) / 2;
            if (_points[mid].TimeSeconds <= timeSeconds)
                lo = mid;
            else
                hi = mid;
        }
        var a = _points[lo];
        var b = _points[hi];
        var t = (timeSeconds - a.TimeSeconds) / (b.TimeSeconds - a.TimeSeconds);
        return a.Value + (b.Value - a.Value) * t;
    }
}
