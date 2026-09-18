using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class ContinuousContactDetector
{
    public static double? TimeOfImpactSpherePlane(
        in Vec3 center0,
        in Vec3 center1,
        double radius,
        in Vec3 planePoint,
        in Vec3 normal
    )
    {
        var d0 = Vec3.Dot(center0 - planePoint, normal) - radius;
        var d1 = Vec3.Dot(center1 - planePoint, normal) - radius;
        if (d0 <= 0)
            return 0;
        if (d1 >= d0 || d1 > 0)
            return null;
        return Math.Clamp(d0 / (d0 - d1), 0, 1);
    }
}
