using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed record LocalMeshSizingRegion(
    string Id,
    BoundingBox3 Bounds,
    double TargetCellSizeMeters,
    int Priority = 0
)
{
    public void Validate()
    {
        if (TargetCellSizeMeters <= 0)
            throw new InvalidOperationException("Target cell size must be positive.");
    }
}
