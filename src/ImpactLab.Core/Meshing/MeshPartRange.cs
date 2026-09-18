using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed record MeshPartRange(
    string PartId,
    int NodeStart,
    int NodeCount,
    int SpringStart,
    int SpringCount,
    BoundingBox3 Bounds
)
{
    public int NodeEndExclusive => NodeStart + NodeCount;
    public int SpringEndExclusive => SpringStart + SpringCount;
}
