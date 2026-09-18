using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct TriangleClosestPointResult(
    Vec3 Point,
    double U,
    double V,
    double W,
    double Distance
);

public static class TriangleClosestPoint
{
    public static TriangleClosestPointResult Evaluate(in Vec3 p, in Vec3 a, in Vec3 b, in Vec3 c)
    {
        var queryPoint = p;
        var ab = b - a;
        var ac = c - a;
        var ap = queryPoint - a;
        var d1 = Vec3.Dot(ab, ap);
        var d2 = Vec3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0)
            return R(a, 1, 0, 0);
        var bp = queryPoint - b;
        var d3 = Vec3.Dot(ab, bp);
        var d4 = Vec3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3)
            return R(b, 0, 1, 0);
        var vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            var v = d1 / (d1 - d3);
            return R(a + ab * v, 1 - v, v, 0);
        }
        var cp = queryPoint - c;
        var d5 = Vec3.Dot(ab, cp);
        var d6 = Vec3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6)
            return R(c, 0, 0, 1);
        var vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            var w = d2 / (d2 - d6);
            return R(a + ac * w, 1 - w, 0, w);
        }
        var va = d3 * d6 - d5 * d4;
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return R(b + (c - b) * w, 0, 1 - w, w);
        }
        var denom = 1.0 / (va + vb + vc);
        var v2 = vb * denom;
        var w2 = vc * denom;
        return R(a + ab * v2 + ac * w2, 1 - v2 - w2, v2, w2);
        TriangleClosestPointResult R(Vec3 q, double u, double v, double w) =>
            new(q, u, v, w, (queryPoint - q).Length);
    }
}
