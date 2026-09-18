namespace ImpactLab.Core.Thermal;

public sealed class TemperatureDependentProperty
{
    private readonly TemperaturePropertyPoint[] _points;

    public TemperatureDependentProperty(IEnumerable<TemperaturePropertyPoint> points)
    {
        _points = points.OrderBy(point => point.TemperatureKelvin).ToArray();
        if (_points.Length == 0)
            throw new ArgumentException(
                "At least one temperature point is required.",
                nameof(points)
            );
    }

    public double Evaluate(double temperatureKelvin)
    {
        if (temperatureKelvin <= _points[0].TemperatureKelvin)
            return _points[0].Value;

        for (var index = 1; index < _points.Length; index++)
        {
            if (temperatureKelvin > _points[index].TemperatureKelvin)
                continue;
            var lower = _points[index - 1];
            var upper = _points[index];
            var fraction =
                (temperatureKelvin - lower.TemperatureKelvin)
                / (upper.TemperatureKelvin - lower.TemperatureKelvin);
            return lower.Value + (upper.Value - lower.Value) * fraction;
        }
        return _points[^1].Value;
    }
}
