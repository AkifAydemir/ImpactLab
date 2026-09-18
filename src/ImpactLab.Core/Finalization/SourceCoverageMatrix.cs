namespace ImpactLab.Core.Finalization;

public sealed class SourceCoverageMatrix
{
    private readonly List<SourceArtifactRequirement> _items = [];
    public IReadOnlyList<SourceArtifactRequirement> Items => _items;

    public void Add(SourceArtifactRequirement item)
    {
        if (_items.Any(x => string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Duplicate source coverage id: {item.Id}");
        if (_items.Any(x => string.Equals(x.Path, item.Path, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Duplicate source coverage path: {item.Path}");
        _items.Add(item);
    }

    public IReadOnlyList<SourceArtifactRequirement> For(FeatureArea area) =>
        _items.Where(x => x.Area == area).OrderBy(x => x.Id).ToArray();
}
