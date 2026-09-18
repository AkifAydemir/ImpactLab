namespace ImpactLab.Core.Parameters;

public sealed class ParameterSchema
{
    private readonly Dictionary<string, ParameterDefinition> _definitions;

    public ParameterSchema(
        string id,
        string displayName,
        IEnumerable<ParameterDefinition> definitions
    )
    {
        Id = string.IsNullOrWhiteSpace(id)
            ? throw new ArgumentException("Schema id is required.", nameof(id))
            : id;
        DisplayName = displayName;
        _definitions = definitions.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in _definitions.Values)
            definition.ValidateDefinition();
    }

    public string Id { get; }
    public string DisplayName { get; }
    public IReadOnlyCollection<ParameterDefinition> Definitions => _definitions.Values;

    public ParameterDefinition Get(string key) =>
        _definitions.TryGetValue(key, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Unknown parameter '{key}' in schema '{Id}'.");

    public ParameterSet CreateDefaults()
    {
        var set = new ParameterSet();
        foreach (var definition in _definitions.Values)
            set.Set(definition.Key, definition.DefaultValue);
        return set;
    }

    public IReadOnlyList<ParameterValidationIssue> Validate(ParameterSet values)
    {
        var issues = new List<ParameterValidationIssue>();
        foreach (var definition in _definitions.Values)
        {
            if (!values.TryGet(definition.Key, out var value))
            {
                issues.Add(new(definition.Key, "Required parameter is missing."));
                continue;
            }
            if (value < definition.Minimum || value > definition.Maximum)
                issues.Add(
                    new(
                        definition.Key,
                        $"Value must be in [{definition.Minimum}, {definition.Maximum}].",
                        value
                    )
                );
        }
        return issues;
    }
}
