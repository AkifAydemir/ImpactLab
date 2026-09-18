namespace ImpactLab.Core.Meshing;

public sealed class SurfaceNodeSet
{
    private readonly Dictionary<string, int[]> _byPart = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, int[]> ByPart => _byPart;

    public void Set(string partId, IEnumerable<int> nodeIds) =>
        _byPart[partId] = nodeIds.Distinct().OrderBy(x => x).ToArray();

    public IReadOnlyList<int> Get(string partId) =>
        _byPart.TryGetValue(partId, out var nodes) ? nodes : Array.Empty<int>();
}
