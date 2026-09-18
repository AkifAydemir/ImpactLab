using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class TransformedGeometrySpec : GeometrySpec
{
    public TransformedGeometrySpec(string name, GeometrySpec inner, GeometryTransform3 transform)
        : base(name, transform.TransformPoint(inner.Center))
    {
        Inner = inner;
        Transform = transform;
        transform.Validate();
        Bounds = ComputeBounds(inner.Bounds, transform);
    }

    public GeometrySpec Inner { get; }
    public GeometryTransform3 Transform { get; }
    public override GeometryKind Kind => Inner.Kind;
    public override BoundingBox3 Bounds { get; }

    public override bool Contains(in Vec3 point) => Inner.Contains(Transform.InversePoint(point));

    private static BoundingBox3 ComputeBounds(BoundingBox3 b, GeometryTransform3 t)
    {
        var p = new List<Vec3>();
        foreach (var x in new[] { b.Min.X, b.Max.X })
        foreach (var y in new[] { b.Min.Y, b.Max.Y })
        foreach (var z in new[] { b.Min.Z, b.Max.Z })
            p.Add(t.TransformPoint(new Vec3(x, y, z)));
        return new BoundingBox3(
            new Vec3(p.Min(v => v.X), p.Min(v => v.Y), p.Min(v => v.Z)),
            new Vec3(p.Max(v => v.X), p.Max(v => v.Y), p.Max(v => v.Z))
        );
    }
}
