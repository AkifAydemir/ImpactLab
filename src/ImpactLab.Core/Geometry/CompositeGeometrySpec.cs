using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class CompositeGeometrySpec : GeometrySpec
{
    public CompositeGeometrySpec(
        string name,
        GeometrySpec a,
        GeometrySpec b,
        BooleanGeometryOperation operation
    )
        : base(name, a.Center)
    {
        A = a;
        B = b;
        Operation = operation;
        Bounds = operation switch
        {
            BooleanGeometryOperation.Union => Union(a.Bounds, b.Bounds),
            BooleanGeometryOperation.Intersection => Intersect(a.Bounds, b.Bounds),
            BooleanGeometryOperation.Subtract => a.Bounds,
            _ => a.Bounds,
        };
    }

    public GeometrySpec A { get; }
    public GeometrySpec B { get; }
    public BooleanGeometryOperation Operation { get; }
    public override GeometryKind Kind => A.Kind;
    public override BoundingBox3 Bounds { get; }

    public override bool Contains(in Vec3 p) =>
        Operation switch
        {
            BooleanGeometryOperation.Union => A.Contains(p) || B.Contains(p),
            BooleanGeometryOperation.Intersection => A.Contains(p) && B.Contains(p),
            BooleanGeometryOperation.Subtract => A.Contains(p) && !B.Contains(p),
            _ => false,
        };

    private static BoundingBox3 Union(BoundingBox3 a, BoundingBox3 b) =>
        new(
            new Vec3(
                Math.Min(a.Min.X, b.Min.X),
                Math.Min(a.Min.Y, b.Min.Y),
                Math.Min(a.Min.Z, b.Min.Z)
            ),
            new Vec3(
                Math.Max(a.Max.X, b.Max.X),
                Math.Max(a.Max.Y, b.Max.Y),
                Math.Max(a.Max.Z, b.Max.Z)
            )
        );

    private static BoundingBox3 Intersect(BoundingBox3 a, BoundingBox3 b) =>
        new(
            new Vec3(
                Math.Max(a.Min.X, b.Min.X),
                Math.Max(a.Min.Y, b.Min.Y),
                Math.Max(a.Min.Z, b.Min.Z)
            ),
            new Vec3(
                Math.Min(a.Max.X, b.Max.X),
                Math.Min(a.Max.Y, b.Max.Y),
                Math.Min(a.Max.Z, b.Max.Z)
            )
        );
}
