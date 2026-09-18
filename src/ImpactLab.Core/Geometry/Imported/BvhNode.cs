using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class BvhNode
{
    public required BoundingBox3 Bounds { get; init; }
    public BvhNode? Left { get; init; }
    public BvhNode? Right { get; init; }
    public int[] TriangleIndices { get; init; } = [];
    public bool IsLeaf => Left is null && Right is null;
}
