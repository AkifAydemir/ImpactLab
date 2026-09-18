namespace ImpactLab.Core.Mathematics;

public readonly struct Matrix3
{
    public Matrix3(
        double m11,
        double m12,
        double m13,
        double m21,
        double m22,
        double m23,
        double m31,
        double m32,
        double m33
    )
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    public double M11 { get; }
    public double M12 { get; }
    public double M13 { get; }
    public double M21 { get; }
    public double M22 { get; }
    public double M23 { get; }
    public double M31 { get; }
    public double M32 { get; }
    public double M33 { get; }
    public static Matrix3 Identity { get; } = Diagonal(1.0, 1.0, 1.0);

    public static Matrix3 Diagonal(double x, double y, double z) => new(x, 0, 0, 0, y, 0, 0, 0, z);

    public Vec3 Multiply(in Vec3 v) =>
        new(
            M11 * v.X + M12 * v.Y + M13 * v.Z,
            M21 * v.X + M22 * v.Y + M23 * v.Z,
            M31 * v.X + M32 * v.Y + M33 * v.Z
        );

    public Matrix3 InverseDiagonal(double epsilon = 1e-18) =>
        Diagonal(
            Math.Abs(M11) <= epsilon ? 0.0 : 1.0 / M11,
            Math.Abs(M22) <= epsilon ? 0.0 : 1.0 / M22,
            Math.Abs(M33) <= epsilon ? 0.0 : 1.0 / M33
        );

    public static Vec3 operator *(in Matrix3 matrix, in Vec3 vector) => matrix.Multiply(vector);
}
