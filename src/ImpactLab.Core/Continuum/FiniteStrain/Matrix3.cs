namespace ImpactLab.Core.Continuum.FiniteStrain;

public readonly record struct Matrix3(
    double M11,
    double M12,
    double M13,
    double M21,
    double M22,
    double M23,
    double M31,
    double M32,
    double M33
)
{
    public static Matrix3 Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);
    public double Determinant =>
        M11 * (M22 * M33 - M23 * M32)
        - M12 * (M21 * M33 - M23 * M31)
        + M13 * (M21 * M32 - M22 * M31);

    public Matrix3 Transpose() => new(M11, M21, M31, M12, M22, M32, M13, M23, M33);

    public Matrix3 Inverse()
    {
        var d = Determinant;
        if (Math.Abs(d) < 1e-18)
            throw new InvalidOperationException("Singular Matrix3.");
        return new(
            (M22 * M33 - M23 * M32) / d,
            (M13 * M32 - M12 * M33) / d,
            (M12 * M23 - M13 * M22) / d,
            (M23 * M31 - M21 * M33) / d,
            (M11 * M33 - M13 * M31) / d,
            (M13 * M21 - M11 * M23) / d,
            (M21 * M32 - M22 * M31) / d,
            (M12 * M31 - M11 * M32) / d,
            (M11 * M22 - M12 * M21) / d
        );
    }

    public static Matrix3 operator +(Matrix3 a, Matrix3 b) =>
        new(
            a.M11 + b.M11,
            a.M12 + b.M12,
            a.M13 + b.M13,
            a.M21 + b.M21,
            a.M22 + b.M22,
            a.M23 + b.M23,
            a.M31 + b.M31,
            a.M32 + b.M32,
            a.M33 + b.M33
        );

    public static Matrix3 operator -(Matrix3 a, Matrix3 b) =>
        new(
            a.M11 - b.M11,
            a.M12 - b.M12,
            a.M13 - b.M13,
            a.M21 - b.M21,
            a.M22 - b.M22,
            a.M23 - b.M23,
            a.M31 - b.M31,
            a.M32 - b.M32,
            a.M33 - b.M33
        );

    public static Matrix3 operator *(Matrix3 a, double s) =>
        new(
            a.M11 * s,
            a.M12 * s,
            a.M13 * s,
            a.M21 * s,
            a.M22 * s,
            a.M23 * s,
            a.M31 * s,
            a.M32 * s,
            a.M33 * s
        );

    public static Matrix3 operator *(Matrix3 a, Matrix3 b) =>
        new(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
            a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
            a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
            a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
            a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
            a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
            a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
        );

    public double Trace => M11 + M22 + M33;
}
