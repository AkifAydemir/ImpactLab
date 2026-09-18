namespace ImpactLab.Core.Parameters;

public sealed class ParameterSet
{
    private readonly Dictionary<string, double> _values = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, double> Values => _values;

    public double Get(string key) =>
        _values.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"Parameter not found: {key}");

    public double GetOrDefault(string key, double fallback) =>
        _values.TryGetValue(key, out var value) ? value : fallback;

    public bool TryGet(string key, out double value) => _values.TryGetValue(key, out value);

    public void Set(string key, double value) => _values[key] = value;

    public bool Remove(string key) => _values.Remove(key);

    public ParameterSet Clone()
    {
        var clone = new ParameterSet();
        foreach (var pair in _values)
            clone.Set(pair.Key, pair.Value);
        return clone;
    }

    public void ApplySchema(ParameterSchema schema, bool clampValues = true)
    {
        foreach (var definition in schema.Definitions)
        {
            var current = GetOrDefault(definition.Key, definition.DefaultValue);
            Set(definition.Key, clampValues ? definition.Normalize(current) : current);
        }
    }
}
