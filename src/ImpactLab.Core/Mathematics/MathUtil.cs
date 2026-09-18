namespace ImpactLab.Core.Mathematics;

public static class MathUtil
{
    public const double Epsilon = 1e-12;

    public static double SafeDivide(double numerator, double denominator, double fallback = 0.0) =>
        Math.Abs(denominator) <= Epsilon ? fallback : numerator / denominator;

    public static double SmoothStep(double edge0, double edge1, double value)
    {
        if (Math.Abs(edge1 - edge0) <= Epsilon)
            return value >= edge1 ? 1.0 : 0.0;
        var t = Math.Clamp((value - edge0) / (edge1 - edge0), 0.0, 1.0);
        return t * t * (3.0 - 2.0 * t);
    }
}
