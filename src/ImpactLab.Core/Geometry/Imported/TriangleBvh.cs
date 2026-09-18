using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class TriangleBvh
{
    private readonly TriangleMeshAsset _asset;

    public TriangleBvh(TriangleMeshAsset asset, int leafSize = 12)
    {
        _asset = asset;
        LeafSize = Math.Max(2, leafSize);
        Root = Build(Enumerable.Range(0, asset.TriangleCount).ToArray());
    }

    public int LeafSize { get; }
    public BvhNode Root { get; }

    public int CountRayHits(in Ray3 ray, double minDistance = 1e-10)
    {
        var r = ray.Normalized();
        var count = 0;
        var stack = new Stack<BvhNode>();
        stack.Push(Root);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (!RayBox(r, n.Bounds))
                continue;
            if (n.IsLeaf)
            {
                foreach (var ti in n.TriangleIndices)
                    if (RayTriangle(r, Triangle(ti), out var t) && t > minDistance)
                        count++;
            }
            else
            {
                if (n.Left is not null)
                    stack.Push(n.Left);
                if (n.Right is not null)
                    stack.Push(n.Right);
            }
        }
        return count;
    }

    public bool Intersects(in Ray3 ray, out double nearest)
    {
        var r = ray.Normalized();
        nearest = double.PositiveInfinity;
        var hit = false;
        var stack = new Stack<BvhNode>();
        stack.Push(Root);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (!RayBox(r, n.Bounds))
                continue;
            if (n.IsLeaf)
            {
                foreach (var ti in n.TriangleIndices)
                    if (RayTriangle(r, Triangle(ti), out var t) && t > 1e-10 && t < nearest)
                    {
                        nearest = t;
                        hit = true;
                    }
            }
            else
            {
                if (n.Left is not null)
                    stack.Push(n.Left);
                if (n.Right is not null)
                    stack.Push(n.Right);
            }
        }
        return hit;
    }

    private BvhNode Build(int[] ids)
    {
        var bounds = Bounds(ids);
        if (ids.Length <= LeafSize)
            return new BvhNode { Bounds = bounds, TriangleIndices = ids };
        var size = bounds.Size;
        var axis =
            size.X >= size.Y && size.X >= size.Z ? 0
            : size.Y >= size.Z ? 1
            : 2;
        Array.Sort(ids, (a, b) => Coord(Centroid(a), axis).CompareTo(Coord(Centroid(b), axis)));
        var mid = ids.Length / 2;
        return new BvhNode
        {
            Bounds = bounds,
            Left = Build(ids[..mid]),
            Right = Build(ids[mid..]),
        };
    }

    private BoundingBox3 Bounds(IEnumerable<int> ids)
    {
        var list = ids.SelectMany(i => new[] { Triangle(i).A, Triangle(i).B, Triangle(i).C })
            .ToArray();
        return new BoundingBox3(
            new Vec3(list.Min(x => x.X), list.Min(x => x.Y), list.Min(x => x.Z)),
            new Vec3(list.Max(x => x.X), list.Max(x => x.Y), list.Max(x => x.Z))
        );
    }

    private Triangle3 Triangle(int i)
    {
        var o = i * 3;
        return new(
            _asset.Vertices[_asset.Indices[o]],
            _asset.Vertices[_asset.Indices[o + 1]],
            _asset.Vertices[_asset.Indices[o + 2]]
        );
    }

    private Vec3 Centroid(int i)
    {
        var t = Triangle(i);
        return (t.A + t.B + t.C) / 3.0;
    }

    private static double Coord(Vec3 v, int a) =>
        a == 0 ? v.X
        : a == 1 ? v.Y
        : v.Z;

    private static bool RayBox(in Ray3 r, BoundingBox3 b)
    {
        double tmin = double.NegativeInfinity,
            tmax = double.PositiveInfinity;
        foreach (var axis in new[] { 0, 1, 2 })
        {
            var o = Coord(r.Origin, axis);
            var d = Coord(r.Direction, axis);
            var mn = Coord(b.Min, axis);
            var mx = Coord(b.Max, axis);
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
            tmin = Math.Max(tmin, a);
            tmax = Math.Min(tmax, c);
            if (tmin > tmax)
                return false;
        }
        return tmax >= Math.Max(0, tmin);
    }

    private static bool RayTriangle(in Ray3 ray, in Triangle3 tri, out double distance)
    {
        const double eps = 1e-12;
        var e1 = tri.B - tri.A;
        var e2 = tri.C - tri.A;
        var h = Vec3.Cross(ray.Direction, e2);
        var a = Vec3.Dot(e1, h);
        if (Math.Abs(a) < eps)
        {
            distance = 0;
            return false;
        }
        var f = 1 / a;
        var s = ray.Origin - tri.A;
        var u = f * Vec3.Dot(s, h);
        if (u < 0 || u > 1)
        {
            distance = 0;
            return false;
        }
        var q = Vec3.Cross(s, e1);
        var v = f * Vec3.Dot(ray.Direction, q);
        if (v < 0 || u + v > 1)
        {
            distance = 0;
            return false;
        }
        distance = f * Vec3.Dot(e2, q);
        return distance > eps;
    }
}
