namespace ImpactLab.Core.Extensions;

public sealed class ExtensionRegistry
{
    private readonly Dictionary<string, IImpactLabExtension> _extensions = new(
        StringComparer.OrdinalIgnoreCase
    );
    public IReadOnlyCollection<IImpactLabExtension> Extensions => _extensions.Values;

    public void Add(IImpactLabExtension extension)
    {
        extension.Manifest.Validate();
        if (!_extensions.TryAdd(extension.Manifest.Id, extension))
            throw new InvalidOperationException($"Duplicate extension id: {extension.Manifest.Id}");
    }

    public bool TryGet(string id, out IImpactLabExtension extension) =>
        _extensions.TryGetValue(id, out extension!);
}
