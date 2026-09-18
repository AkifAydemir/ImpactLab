using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public readonly record struct GeometryTransform3(Vec3 Translation, QuaternionD Rotation, Vec3 Scale)
{
    public static GeometryTransform3 Identity =>
        new(Vec3.Zero, QuaternionD.Identity, new Vec3(1, 1, 1));

    public Vec3 TransformPoint(in Vec3 p) =>
        Rotation.Rotate(new Vec3(p.X * Scale.X, p.Y * Scale.Y, p.Z * Scale.Z)) + Translation;

    public Vec3 InversePoint(in Vec3 p)
    {
        var q = Rotation.InverseRotate(p - Translation);
        return new Vec3(q.X / Scale.X, q.Y / Scale.Y, q.Z / Scale.Z);
    }

    public void Validate()
    {
        if (Math.Abs(Scale.X) < 1e-12 || Math.Abs(Scale.Y) < 1e-12 || Math.Abs(Scale.Z) < 1e-12)
            throw new InvalidOperationException("Geometry scale components must be non-zero.");
    }
}
