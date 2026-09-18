namespace ImpactLab.Core.Materials;

public sealed class MaterialInterfaceTable
{
    private readonly Dictionary<(string A, string B), MaterialInterfaceDefinition> _rules = [];

    public MaterialInterfaceTable(MaterialInterfaceDefinition? defaultInterface = null)
    {
        DefaultInterface = defaultInterface ?? MaterialInterfaceDefinition.Neutral;
        DefaultInterface.Validate();
    }

    public MaterialInterfaceDefinition DefaultInterface { get; }

    public void Set(string materialAId, string materialBId, MaterialInterfaceDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(materialAId) || string.IsNullOrWhiteSpace(materialBId))
            throw new ArgumentException("Material ids cannot be empty.");
        definition.Validate();
        _rules[Normalize(materialAId, materialBId)] = definition;
    }

    public MaterialInterfaceDefinition? Resolve(MaterialDefinition a, MaterialDefinition b)
    {
        if (string.Equals(a.Id, b.Id, StringComparison.Ordinal))
            return null;
        return _rules.TryGetValue(Normalize(a.Id, b.Id), out var value) ? value : DefaultInterface;
    }

    private static (string A, string B) Normalize(string a, string b) =>
        string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);
}
