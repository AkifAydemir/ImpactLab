using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class AngleProfileSpec : GeometrySpec
{
    public AngleProfileSpec(
        string name,
        Vec3 center,
        double legX,
        double legY,
        double thickness,
        double length
    )
        : base(name, center)
    {
        if (legX <= 0.0 || legY <= 0.0 || thickness <= 0.0 || length <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(legX));
        if (thickness >= legX || thickness >= legY)
            throw new ArgumentOutOfRangeException(nameof(thickness));
        LegX = legX;
        LegY = legY;
        Thickness = thickness;
        Length = length;
    }

    public double LegX { get; }
    public double LegY { get; }
    public double Thickness { get; }
    public double Length { get; }
    public override GeometryKind Kind => GeometryKind.AngleProfile;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = new Vec3(LegX * 0.5, LegY * 0.5, Length * 0.5);
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point)
    {
        var p = point - Center;
        if (Math.Abs(p.Z) > Length * 0.5)
            return false;
        var insideOuter = Math.Abs(p.X) <= LegX * 0.5 && Math.Abs(p.Y) <= LegY * 0.5;
        if (!insideOuter)
            return false;
        var verticalLeg = p.X <= -LegX * 0.5 + Thickness;
        var horizontalLeg = p.Y <= -LegY * 0.5 + Thickness;
        return verticalLeg || horizontalLeg;
    }
}
