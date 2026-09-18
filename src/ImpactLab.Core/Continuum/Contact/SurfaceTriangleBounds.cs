using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class SurfaceTriangleBounds
{
    public static BoundingBox3 Of(SurfaceTriangle t, ReadOnlySpan<Vec3> p)
    {
        var min = Vec3.Min(p[t.A], Vec3.Min(p[t.B], p[t.C]));
        var max = Vec3.Max(p[t.A], Vec3.Max(p[t.B], p[t.C]));
        return new(min, max);
    }

    public static BoundingBox3 Expand(this BoundingBox3 b, double e) =>
        new(b.Min - new Vec3(e, e, e), b.Max + new Vec3(e, e, e));
}
