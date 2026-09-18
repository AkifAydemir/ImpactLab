namespace ImpactLab.Core.Meshing;

public sealed class SimulationMesh
{
    private readonly Dictionary<(string PartId, int X, int Y, int Z), int> _gridToNode;
    private readonly Dictionary<string, MeshPartRange> _partById;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<int>> _boundaryNodes;

    public SimulationMesh(
        IReadOnlyList<MeshNode> nodes,
        IReadOnlyList<MeshSpring> springs,
        double cellSize,
        Dictionary<(string PartId, int X, int Y, int Z), int> gridToNode,
        IReadOnlyList<MeshPartRange> parts,
        MeshTopologyKind topologyKind = MeshTopologyKind.UniformStructured,
        IReadOnlyDictionary<string, IReadOnlyList<int>>? boundaryNodes = null
    )
    {
        Nodes = nodes;
        Springs = springs;
        CellSize = cellSize;
        _gridToNode = gridToNode;
        Parts = parts;
        TopologyKind = topologyKind;
        _partById = parts.ToDictionary(x => x.PartId, StringComparer.Ordinal);
        _boundaryNodes =
            boundaryNodes
            ?? new Dictionary<string, IReadOnlyList<int>>(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<MeshNode> Nodes { get; }
    public IReadOnlyList<MeshSpring> Springs { get; }
    public double CellSize { get; }
    public IReadOnlyList<MeshPartRange> Parts { get; }
    public MeshTopologyKind TopologyKind { get; }

    public bool TryGetNodeId(string partId, int x, int y, int z, out int nodeId) =>
        _gridToNode.TryGetValue((partId, x, y, z), out nodeId);

    public bool TryGetNode(string partId, int x, int y, int z, out MeshNode? node)
    {
        if (TryGetNodeId(partId, x, y, z, out var id))
        {
            node = Nodes[id];
            return true;
        }
        node = null;
        return false;
    }

    public bool TryGetBoundaryNodes(string partId, out IReadOnlyList<int> ids) =>
        _boundaryNodes.TryGetValue(partId, out ids!);

    public MeshPartRange GetPart(string partId) =>
        _partById.TryGetValue(partId, out var part)
            ? part
            : throw new KeyNotFoundException($"Mesh part not found: {partId}");
}
