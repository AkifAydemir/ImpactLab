using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class SurfaceTriangleBvh
{
    private readonly SurfaceMesh _surface;
    private readonly Node? _root;

    private sealed record Node(BoundingBox3 Bounds, int[]? Indices, Node? Left, Node? Right);

    public SurfaceTriangleBvh(SurfaceMesh surface)
    {
        _surface = surface;
        _root = Build(Enumerable.Range(0, surface.Triangles.Count).ToArray());
    }

    public void Query(in Vec3 point, double radius, List<int> result)
    {
        var queryPoint = point;
        result.Clear();
        Visit(_root);
        void Visit(Node? n)
        {
            if (n is null || Distance(n.Bounds, queryPoint) > radius)
                return;
            if (n.Indices is not null)
            {
                result.AddRange(n.Indices);
                return;
            }
            Visit(n.Left);
            Visit(n.Right);
        }
    }

    private Node? Build(int[] ids)
    {
        if (ids.Length == 0)
            return null;
        var bounds = Bounds(ids);
        if (ids.Length <= 8)
            return new(bounds, ids, null, null);
        var size = bounds.Size;
        var axis =
            size.X >= size.Y && size.X >= size.Z ? 0
            : size.Y >= size.Z ? 1
            : 2;
        Array.Sort(ids, (x, y) => Coord(Centroid(x), axis).CompareTo(Coord(Centroid(y), axis)));
        var mid = ids.Length / 2;
        return new(bounds, null, Build(ids[..mid]), Build(ids[mid..]));
    }

    private BoundingBox3 Bounds(int[] ids) =>
        BoundingBox3.FromPoints(
            ids.SelectMany(i =>
            {
                var t = _surface.Triangles[i];
                return new[]
                {
                    _surface.Volume.Nodes[t.A].Position,
                    _surface.Volume.Nodes[t.B].Position,
                    _surface.Volume.Nodes[t.C].Position,
                };
            })
        );

    private Vec3 Centroid(int i)
    {
        var t = _surface.Triangles[i];
        return (
                _surface.Volume.Nodes[t.A].Position
                + _surface.Volume.Nodes[t.B].Position
                + _surface.Volume.Nodes[t.C].Position
            ) / 3.0;
    }

    private static double Coord(Vec3 p, int a) =>
        a == 0 ? p.X
        : a == 1 ? p.Y
        : p.Z;

    private static double Distance(BoundingBox3 b, Vec3 p)
    {
        var dx = Math.Max(Math.Max(b.Min.X - p.X, 0), p.X - b.Max.X);
        var dy = Math.Max(Math.Max(b.Min.Y - p.Y, 0), p.Y - b.Max.Y);
        var dz = Math.Max(Math.Max(b.Min.Z - p.Z, 0), p.Z - b.Max.Z);
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
