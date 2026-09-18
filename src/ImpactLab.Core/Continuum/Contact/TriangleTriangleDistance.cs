using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class TriangleTriangleDistance
{
    public static double Compute(Vec3 a0, Vec3 a1, Vec3 a2, Vec3 b0, Vec3 b1, Vec3 b2)
    {
        var d = double.PositiveInfinity;
        foreach (var p in new[] { a0, a1, a2 })
            d = Math.Min(d, TriangleClosestPoint.Evaluate(p, b0, b1, b2).Distance);
        foreach (var p in new[] { b0, b1, b2 })
            d = Math.Min(d, TriangleClosestPoint.Evaluate(p, a0, a1, a2).Distance);
        return d;
    }
}
