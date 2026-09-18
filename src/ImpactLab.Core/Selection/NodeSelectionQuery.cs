using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Selection;

public sealed record NodeSelectionQuery
{
    public string? PartId { get; init; }
    public BoundingBox3? Bounds { get; init; }
    public bool SurfaceOnly { get; init; }
    public Vec3 PlaneNormal { get; init; } = Vec3.Zero;
    public double PlaneOffset { get; init; }
    public double PlaneToleranceMeters { get; init; }
    public bool UsesPlane => PlaneNormal.LengthSquared > 1e-18;

    public void Validate()
    {
        if (PlaneToleranceMeters < 0.0)
            throw new InvalidOperationException("Plane tolerance cannot be negative.");
        if (UsesPlane && PlaneNormal.Length <= 1e-12)
            throw new InvalidOperationException("Plane normal cannot be zero.");
    }
}
