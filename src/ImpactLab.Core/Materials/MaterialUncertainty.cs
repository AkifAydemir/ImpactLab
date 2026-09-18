namespace ImpactLab.Core.Materials;

public sealed class MaterialUncertainty
{
    private readonly Dictionary<string, MaterialPropertyBand> _properties = new(
        StringComparer.OrdinalIgnoreCase
    );
    public IReadOnlyDictionary<string, MaterialPropertyBand> Properties => _properties;

    public void Set(string property, MaterialPropertyBand band)
    {
        if (string.IsNullOrWhiteSpace(property))
            throw new ArgumentException("Property name cannot be empty.");
        band.Validate();
        _properties[property] = band;
    }

    public bool TryGet(string property, out MaterialPropertyBand band) =>
        _properties.TryGetValue(property, out band!);
}
