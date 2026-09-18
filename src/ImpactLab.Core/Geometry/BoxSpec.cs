using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class BoxSpec : GeometrySpec
{
    public BoxSpec(string name, Vec3 center, Vec3 size)
        : base(name, center)
    {
        if (size.X <= 0.0 || size.Y <= 0.0 || size.Z <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(size));
        Size = size;
    }

    public Vec3 Size { get; }
    public override GeometryKind Kind => GeometryKind.Box;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = Size * 0.5;
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point)
    {
        var p = point - Center;
        return Math.Abs(p.X) <= Size.X * 0.5
            && Math.Abs(p.Y) <= Size.Y * 0.5
            && Math.Abs(p.Z) <= Size.Z * 0.5;
    }
}
