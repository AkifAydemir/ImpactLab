namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class Matrix3Math
{
    public static double Frobenius(Matrix3 a) =>
        Math.Sqrt(
            a.M11 * a.M11
                + a.M12 * a.M12
                + a.M13 * a.M13
                + a.M21 * a.M21
                + a.M22 * a.M22
                + a.M23 * a.M23
                + a.M31 * a.M31
                + a.M32 * a.M32
                + a.M33 * a.M33
        );

    public static Matrix3 Symmetric(Matrix3 a) => (a + a.Transpose()) * 0.5;

    public static Matrix3 Skew(Matrix3 a) => (a - a.Transpose()) * 0.5;

    public static Matrix3 Deviator(Matrix3 a) => a - Matrix3.Identity * (a.Trace / 3.0);
}
