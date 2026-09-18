using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed record AdaptiveRefinementRegion(
    string Id,
    BoundingBox3 Bounds,
    int TargetDepth = 2,
    string Reason = "User focus"
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Refinement region id required.");
        if (TargetDepth < 0)
            throw new InvalidOperationException("Target depth cannot be negative.");
    }
}
