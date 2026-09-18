using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class MortarProjector
{
    public static MortarProjectionResult Project(
        int slaveId,
        int masterId,
        Vec3 p,
        Vec3 a,
        Vec3 b,
        Vec3 c
    )
    {
        var v0 = b - a;
        var v1 = c - a;
        var v2 = p - a;
        var d00 = Vec3.Dot(v0, v0);
        var d01 = Vec3.Dot(v0, v1);
        var d11 = Vec3.Dot(v1, v1);
        var d20 = Vec3.Dot(v2, v0);
        var d21 = Vec3.Dot(v2, v1);
        var den = d00 * d11 - d01 * d01;
        if (Math.Abs(den) < 1e-20)
            return new(slaveId, masterId, 0, 0, 0, 0, false);
        var xi = (d11 * d20 - d01 * d21) / den;
        var eta = (d00 * d21 - d01 * d20) / den;
        var n = Vec3.Cross(v0, v1).Normalized();
        var q = a + v0 * xi + v1 * eta;
        var gap = Vec3.Dot(p - q, n);
        return new(
            slaveId,
            masterId,
            xi,
            eta,
            gap,
            0.5 * Vec3.Cross(v0, v1).Length,
            xi >= 0 && eta >= 0 && xi + eta <= 1
        );
    }
}
