using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class SweptTriangleBounds
{
    public static BoundingBox3 Build(
        in Vec3 a0,
        in Vec3 b0,
        in Vec3 c0,
        in Vec3 a1,
        in Vec3 b1,
        in Vec3 c1,
        double pad
    )
    {
        var min = new Vec3(
            Math.Min(Math.Min(Math.Min(a0.X, b0.X), c0.X), Math.Min(Math.Min(a1.X, b1.X), c1.X)),
            Math.Min(Math.Min(Math.Min(a0.Y, b0.Y), c0.Y), Math.Min(Math.Min(a1.Y, b1.Y), c1.Y)),
            Math.Min(Math.Min(Math.Min(a0.Z, b0.Z), c0.Z), Math.Min(Math.Min(a1.Z, b1.Z), c1.Z))
        );
        var max = new Vec3(
            Math.Max(Math.Max(Math.Max(a0.X, b0.X), c0.X), Math.Max(Math.Max(a1.X, b1.X), c1.X)),
            Math.Max(Math.Max(Math.Max(a0.Y, b0.Y), c0.Y), Math.Max(Math.Max(a1.Y, b1.Y), c1.Y)),
            Math.Max(Math.Max(Math.Max(a0.Z, b0.Z), c0.Z), Math.Max(Math.Max(a1.Z, b1.Z), c1.Z))
        );
        var p = new Vec3(pad, pad, pad);
        return new(min - p, max + p);
    }
}
