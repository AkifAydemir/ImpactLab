namespace ImpactLab.Core.Selection;

public sealed class NodeSelection
{
    private readonly int[] _nodeIds;

    public NodeSelection(string id, IEnumerable<int> nodeIds)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Selection id cannot be empty.", nameof(id));
        Id = id;
        _nodeIds = nodeIds.Distinct().OrderBy(x => x).ToArray();
    }

    public string Id { get; }
    public IReadOnlyList<int> NodeIds => _nodeIds;
    public int Count => _nodeIds.Length;
    public bool IsEmpty => _nodeIds.Length == 0;
}
