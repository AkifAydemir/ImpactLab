using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public sealed record TransformDelta(Vec3 Translation, Vec3 RotationDegrees, Vec3 Scale)
{
    public static TransformDelta Identity { get; } = new(Vec3.Zero, Vec3.Zero, new Vec3(1, 1, 1));
}
