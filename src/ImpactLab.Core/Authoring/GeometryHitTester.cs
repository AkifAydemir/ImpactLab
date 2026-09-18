using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public static class GeometryHitTester
{
    public static GeometryHit? Hit(
        string partId,
        GeometrySpec geometry,
        Ray3 ray,
        double maxDistance = 1e6
    )
    {
        if (
            geometry is AcceleratedTriangleMeshSpec imported
            && imported.Raycast(ray, out var t)
            && t <= maxDistance
        )
            return new(partId, t, ray.PointAt(t), Vec3.Zero);
        if (!RayBox(ray, geometry.Bounds, out var near) || near > maxDistance)
            return null;
        var p = ray.PointAt(Math.Max(near, 0));
        return new(partId, Math.Max(near, 0), p, EstimateNormal(geometry.Bounds, p));
    }

    private static bool RayBox(Ray3 r, BoundingBox3 b, out double near)
    {
        near = 0;
        var far = double.PositiveInfinity;
        foreach (var axis in new[] { 0, 1, 2 })
        {
            double o =
                axis == 0 ? r.Origin.X
                : axis == 1 ? r.Origin.Y
                : r.Origin.Z;
            double d =
                axis == 0 ? r.Direction.X
                : axis == 1 ? r.Direction.Y
                : r.Direction.Z;
            double mn =
                axis == 0 ? b.Min.X
                : axis == 1 ? b.Min.Y
                : b.Min.Z;
            double mx =
                axis == 0 ? b.Max.X
                : axis == 1 ? b.Max.Y
                : b.Max.Z;
            if (Math.Abs(d) < 1e-15)
            {
                if (o < mn || o > mx)
                    return false;
                continue;
            }
            var a = (mn - o) / d;
            var c = (mx - o) / d;
            if (a > c)
                (a, c) = (c, a);
            near = Math.Max(near, a);
            far = Math.Min(far, c);
            if (near > far)
                return false;
        }
        return far >= 0;
    }

    private static Vec3 EstimateNormal(BoundingBox3 b, Vec3 p)
    {
        var d = new[]
        {
            (Math.Abs(p.X - b.Min.X), new Vec3(-1, 0, 0)),
            (Math.Abs(p.X - b.Max.X), Vec3.UnitX),
            (Math.Abs(p.Y - b.Min.Y), new Vec3(0, -1, 0)),
            (Math.Abs(p.Y - b.Max.Y), Vec3.UnitY),
            (Math.Abs(p.Z - b.Min.Z), new Vec3(0, 0, -1)),
            (Math.Abs(p.Z - b.Max.Z), Vec3.UnitZ),
        };
        return d.OrderBy(x => x.Item1).First().Item2;
    }
}
