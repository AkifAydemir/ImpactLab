using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Rigid;

public static class RigidBodyInertia
{
    public static Matrix3 Approximate(GeometrySpec geometry, double massKg) =>
        geometry switch
        {
            SphereSpec sphere => Sphere(massKg, sphere.Radius),
            BoxSpec box => Box(massKg, box.Size),
            RectangularPlateSpec plate => Box(
                massKg,
                new Vec3(plate.Width, plate.Height, plate.Thickness)
            ),
            CylinderSpec cylinder => CylinderZ(massKg, cylinder.Radius, cylinder.Length),
            TubeSpec tube => TubeZ(massKg, tube.InnerRadius, tube.OuterRadius, tube.Length),
            _ => Box(massKg, geometry.Bounds.Size),
        };

    private static Matrix3 Sphere(double mass, double radius)
    {
        var i = 0.4 * mass * radius * radius;
        return Matrix3.Diagonal(i, i, i);
    }

    private static Matrix3 Box(double mass, in Vec3 size)
    {
        var x2 = size.X * size.X;
        var y2 = size.Y * size.Y;
        var z2 = size.Z * size.Z;
        return Matrix3.Diagonal(
            mass * (y2 + z2) / 12.0,
            mass * (x2 + z2) / 12.0,
            mass * (x2 + y2) / 12.0
        );
    }

    private static Matrix3 CylinderZ(double mass, double radius, double length)
    {
        var transverse = mass * (3.0 * radius * radius + length * length) / 12.0;
        var axial = 0.5 * mass * radius * radius;
        return Matrix3.Diagonal(transverse, transverse, axial);
    }

    private static Matrix3 TubeZ(double mass, double inner, double outer, double length)
    {
        var radial2 = inner * inner + outer * outer;
        var transverse = mass * (3.0 * radial2 + length * length) / 12.0;
        var axial = 0.5 * mass * radial2;
        return Matrix3.Diagonal(transverse, transverse, axial);
    }
}
