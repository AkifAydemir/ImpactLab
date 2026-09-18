using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class SphereSpec : GeometrySpec
{
    public SphereSpec(string name, Vec3 center, double radius)
        : base(name, center)
    {
        if (radius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        Radius = radius;
    }

    public double Radius { get; }
    public override GeometryKind Kind => GeometryKind.Sphere;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = new Vec3(Radius, Radius, Radius);
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point) =>
        (point - Center).LengthSquared <= Radius * Radius;
}
