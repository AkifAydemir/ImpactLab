namespace ImpactLab.Core.Thermal;

public sealed class ThermalPropertyTable
{
    private readonly Dictionary<string, ThermalProperties> _items = new(
        StringComparer.OrdinalIgnoreCase
    );
    public ThermalProperties Default { get; set; } = ThermalProperties.SteelDefault;

    public void Set(string materialId, ThermalProperties properties)
    {
        properties.Validate();
        _items[materialId] = properties;
    }

    public ThermalProperties Resolve(string materialId) =>
        _items.TryGetValue(materialId, out var p) ? p : Default;
}
