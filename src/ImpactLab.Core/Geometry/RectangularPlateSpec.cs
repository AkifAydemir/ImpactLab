using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public sealed class RectangularPlateSpec : GeometrySpec
{
    public RectangularPlateSpec(
        string name,
        Vec3 center,
        double width,
        double height,
        double thickness
    )
        : base(name, center)
    {
        if (width <= 0.0 || height <= 0.0 || thickness <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(width));
        Width = width;
        Height = height;
        Thickness = thickness;
    }

    public double Width { get; }
    public double Height { get; }
    public double Thickness { get; }
    public override GeometryKind Kind => GeometryKind.Plate;
    public override BoundingBox3 Bounds
    {
        get
        {
            var half = new Vec3(Width * 0.5, Height * 0.5, Thickness * 0.5);
            return new BoundingBox3(Center - half, Center + half);
        }
    }

    public override bool Contains(in Vec3 point)
    {
        var p = point - Center;
        return Math.Abs(p.X) <= Width * 0.5
            && Math.Abs(p.Y) <= Height * 0.5
            && Math.Abs(p.Z) <= Thickness * 0.5;
    }
}
