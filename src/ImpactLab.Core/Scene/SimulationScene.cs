namespace ImpactLab.Core.Scene;

public sealed class SimulationScene
{
    private readonly List<ScenePart> _parts = [];
    public IReadOnlyList<ScenePart> Parts => _parts;

    public void Add(ScenePart part)
    {
        ArgumentNullException.ThrowIfNull(part);
        if (_parts.Any(x => string.Equals(x.Id, part.Id, StringComparison.Ordinal)))
            throw new InvalidOperationException($"Duplicate scene part id: {part.Id}");
        _parts.Add(part);
    }

    public ScenePart Get(string id) =>
        _parts.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.Ordinal))
        ?? throw new KeyNotFoundException($"Scene part not found: {id}");
}
