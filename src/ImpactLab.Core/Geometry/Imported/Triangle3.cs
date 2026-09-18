using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public readonly record struct Triangle3(Vec3 A, Vec3 B, Vec3 C)
{
    public Vec3 Normal => Vec3.Cross(B - A, C - A).Normalized();
    public double Area => Vec3.Cross(B - A, C - A).Length * 0.5;
}
