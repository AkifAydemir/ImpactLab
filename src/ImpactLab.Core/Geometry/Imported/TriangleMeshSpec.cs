using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class TriangleMeshSpec : GeometrySpec
{
    public TriangleMeshSpec(string name, TriangleMeshAsset asset, Vec3 translation)
        : base(name, asset.Bounds.Center + translation)
    {
        Asset = asset;
        Translation = translation;
        Bounds = new BoundingBox3(asset.Bounds.Min + translation, asset.Bounds.Max + translation);
    }

    public TriangleMeshAsset Asset { get; }
    public Vec3 Translation { get; }
    public override GeometryKind Kind => GeometryKind.ImportedMesh;
    public override BoundingBox3 Bounds { get; }

    public override bool Contains(in Vec3 point)
    {
        if (!Bounds.Contains(point))
            return false;
        var p = point - Translation;
        var hits = 0;
        var direction = new Vec3(1.0, 0.173205080756, 0.091287092918).Normalized();
        foreach (var tri in Asset.Triangles)
            if (RayIntersects(p, direction, tri, out var t) && t > 1e-10)
                hits++;
        return (hits & 1) == 1;
    }

    private static bool RayIntersects(
        in Vec3 origin,
        in Vec3 direction,
        in Triangle3 tri,
        out double distance
    )
    {
        const double eps = 1e-12;
        var e1 = tri.B - tri.A;
        var e2 = tri.C - tri.A;
        var h = Vec3.Cross(direction, e2);
        var a = Vec3.Dot(e1, h);
        if (Math.Abs(a) < eps)
        {
            distance = 0;
            return false;
        }
        var f = 1.0 / a;
        var s = origin - tri.A;
        var u = f * Vec3.Dot(s, h);
        if (u < 0 || u > 1)
        {
            distance = 0;
            return false;
        }
        var q = Vec3.Cross(s, e1);
        var v = f * Vec3.Dot(direction, q);
        if (v < 0 || u + v > 1)
        {
            distance = 0;
            return false;
        }
        distance = f * Vec3.Dot(e2, q);
        return distance > eps;
    }
}
