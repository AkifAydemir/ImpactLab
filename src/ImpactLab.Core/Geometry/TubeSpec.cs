using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class TubeSpec : GeometrySpec
{
    public TubeSpec(
        string name,
        Vec3 center,
        double outerRadius,
        double wallThickness,
        double length
    )
        : base(name, center)
    {
        if (outerRadius <= 0.0 || length <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(outerRadius));
        if (wallThickness <= 0.0 || wallThickness >= outerRadius)
            throw new ArgumentOutOfRangeException(nameof(wallThickness));
        OuterRadius = outerRadius;
        WallThickness = wallThickness;
        Length = length;
    }

    public double OuterRadius { get; }
    public double WallThickness { get; }
    public double Length { get; }
    public double InnerRadius => OuterRadius - WallThickness;
    public override GeometryKind Kind => GeometryKind.Tube;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = new Vec3(OuterRadius, OuterRadius, Length * 0.5);
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point)
    {
        var p = point - Center;
        var radialSquared = p.X * p.X + p.Y * p.Y;
        return radialSquared <= OuterRadius * OuterRadius
            && radialSquared >= InnerRadius * InnerRadius
            && Math.Abs(p.Z) <= Length * 0.5;
    }
}
