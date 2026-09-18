namespace ImpactLab.App.Accessibility;

public sealed class AccessibilityCatalog
{
    private readonly Dictionary<string, AccessibleCommandDescriptor> _items = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(AccessibleCommandDescriptor d) => _items[d.CommandId] = d;

    public AccessibleCommandDescriptor? Find(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<AccessibleCommandDescriptor> All =>
        _items.Values.OrderBy(x => x.Name).ToArray();
}
