using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public readonly record struct Ray3(Vec3 Origin, Vec3 Direction)
{
    public Ray3 Normalized() => new(Origin, Direction.Normalized());

    public Vec3 PointAt(double t) => Origin + Direction * t;
}
