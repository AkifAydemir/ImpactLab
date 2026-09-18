namespace ImpactLab.Core.Analysis;

public sealed class ResultMetricSet
{
    private readonly Dictionary<ResultMetricKind, double> _values = [];
    public IReadOnlyDictionary<ResultMetricKind, double> Values => _values;
    public double this[ResultMetricKind kind] => _values[kind];

    public void Set(ResultMetricKind kind, double value) => _values[kind] = value;

    public bool TryGet(ResultMetricKind kind, out double value) =>
        _values.TryGetValue(kind, out value);
}
