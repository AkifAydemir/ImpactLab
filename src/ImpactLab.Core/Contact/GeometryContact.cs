using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Contact;

public static class GeometryContact
{
    public static ContactSample Sample(
        GeometrySpec geometry,
        in RigidTransform transform,
        in Vec3 point
    )
    {
        var local = transform.InverseTransformPoint(point);
        var geometryLocal = local - geometry.Center;
        var localSample = geometry switch
        {
            SphereSpec sphere => SampleSphere(geometryLocal, sphere.Radius),
            RectangularPlateSpec plate => SampleBox(
                geometryLocal,
                new Vec3(plate.Width, plate.Height, plate.Thickness) * 0.5
            ),
            BoxSpec box => SampleBox(geometryLocal, box.Size * 0.5),
            CylinderSpec cylinder => SampleCylinder(
                geometryLocal,
                cylinder.Radius,
                cylinder.Length * 0.5
            ),
            TubeSpec tube => SampleTube(
                geometryLocal,
                tube.InnerRadius,
                tube.OuterRadius,
                tube.Length * 0.5
            ),
            _ => SampleBox(geometryLocal, geometry.Bounds.Size * 0.5),
        };
        return new ContactSample(
            localSample.SignedDistanceMeters,
            transform.TransformVector(localSample.OutwardNormal).Normalized()
        );
    }

    private static ContactSample SampleSphere(in Vec3 local, double radius)
    {
        var length = local.Length;
        var normal = length <= 1e-12 ? Vec3.UnitZ : local / length;
        return new ContactSample(length - radius, normal);
    }

    private static ContactSample SampleBox(in Vec3 p, in Vec3 half)
    {
        var qx = Math.Abs(p.X) - half.X;
        var qy = Math.Abs(p.Y) - half.Y;
        var qz = Math.Abs(p.Z) - half.Z;
        var ox = Math.Max(qx, 0.0);
        var oy = Math.Max(qy, 0.0);
        var oz = Math.Max(qz, 0.0);
        var outside = Math.Sqrt(ox * ox + oy * oy + oz * oz);
        var inside = Math.Min(Math.Max(qx, Math.Max(qy, qz)), 0.0);
        var distance = outside + inside;
        Vec3 normal;
        if (outside > 1e-12)
        {
            normal = new Vec3(
                ox * SignOrOne(p.X),
                oy * SignOrOne(p.Y),
                oz * SignOrOne(p.Z)
            ).Normalized();
        }
        else if (qx >= qy && qx >= qz)
            normal = new Vec3(SignOrOne(p.X), 0.0, 0.0);
        else if (qy >= qz)
            normal = new Vec3(0.0, SignOrOne(p.Y), 0.0);
        else
            normal = new Vec3(0.0, 0.0, SignOrOne(p.Z));
        return new ContactSample(distance, normal);
    }

    private static ContactSample SampleCylinder(in Vec3 p, double radius, double halfLength)
    {
        var radial = Math.Sqrt(p.X * p.X + p.Y * p.Y);
        var dr = radial - radius;
        var dz = Math.Abs(p.Z) - halfLength;
        var or = Math.Max(dr, 0.0);
        var oz = Math.Max(dz, 0.0);
        var outside = Math.Sqrt(or * or + oz * oz);
        var inside = Math.Min(Math.Max(dr, dz), 0.0);
        var radialNormal = radial <= 1e-12 ? Vec3.UnitX : new Vec3(p.X / radial, p.Y / radial, 0.0);
        var normal =
            outside <= 1e-12
                ? dr >= dz
                    ? radialNormal
                    : new Vec3(0.0, 0.0, SignOrOne(p.Z))
                : (radialNormal * or + new Vec3(0.0, 0.0, SignOrOne(p.Z)) * oz).Normalized();
        return new ContactSample(outside + inside, normal);
    }

    private static ContactSample SampleTube(
        in Vec3 p,
        double innerRadius,
        double outerRadius,
        double halfLength
    )
    {
        var radial = Math.Sqrt(p.X * p.X + p.Y * p.Y);
        var mid = (innerRadius + outerRadius) * 0.5;
        var halfWall = (outerRadius - innerRadius) * 0.5;
        var dr = Math.Abs(radial - mid) - halfWall;
        var dz = Math.Abs(p.Z) - halfLength;
        var radialNormal =
            radial <= 1e-12
                ? Vec3.UnitX
                : new Vec3(p.X / radial, p.Y / radial, 0.0) * SignOrOne(radial - mid);
        var normal = dr >= dz ? radialNormal : new Vec3(0.0, 0.0, SignOrOne(p.Z));
        return new ContactSample(Math.Max(dr, dz), normal.Normalized());
    }

    private static double SignOrOne(double value) => value < 0.0 ? -1.0 : 1.0;
}
