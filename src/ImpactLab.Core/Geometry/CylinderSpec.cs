using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class CylinderSpec : GeometrySpec
{
    public CylinderSpec(string name, Vec3 center, double radius, double length)
        : base(name, center)
    {
        if (radius <= 0.0 || length <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        Radius = radius;
        Length = length;
    }

    public double Radius { get; }
    public double Length { get; }
    public override GeometryKind Kind => GeometryKind.Cylinder;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = new Vec3(Radius, Radius, Length * 0.5);
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point)
    {
        var p = point - Center;
        var radialSquared = p.X * p.X + p.Y * p.Y;
        return radialSquared <= Radius * Radius && Math.Abs(p.Z) <= Length * 0.5;
    }
}
