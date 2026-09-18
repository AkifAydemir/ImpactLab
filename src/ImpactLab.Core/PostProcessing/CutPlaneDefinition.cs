using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.PostProcessing;

public sealed record CutPlaneDefinition(
    string Id,
    Vec3 Normal,
    double Offset,
    double HalfThicknessMeters
)
{
    public Vec3 UnitNormal => Normal.Normalized();

    public void Validate()
    {
        if (Normal.Length <= 1e-12)
            throw new InvalidOperationException("Cut-plane normal cannot be zero.");
        if (HalfThicknessMeters < 0)
            throw new InvalidOperationException("Cut-plane thickness cannot be negative.");
    }
}
