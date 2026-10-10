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

    public Matrix3 InverseDiagonal(double relativeTolerance = 1e-12)
    {
        var scale = Math.Max(Math.Abs(M11), Math.Max(Math.Abs(M22), Math.Abs(M33)));
        var threshold = relativeTolerance * Math.Max(scale, double.Epsilon);
        if (Math.Abs(M11) <= threshold || Math.Abs(M22) <= threshold || Math.Abs(M33) <= threshold)
            throw new InvalidOperationException(
                "Cannot invert a singular or ill-conditioned diagonal matrix."
            );
        return Diagonal(1.0 / M11, 1.0 / M22, 1.0 / M33);
    }

    public double Determinant =>
        M11 * (M22 * M33 - M23 * M32)
        - M12 * (M21 * M33 - M23 * M31)
        + M13 * (M21 * M32 - M22 * M31);

    public bool TryInverse(
        out Matrix3 inverse,
        out double conditionNumber,
        double maximumConditionNumber = 1e12,
        double relativeTolerance = 1e-12
    )
    {
        if (maximumConditionNumber < 1.0)
            throw new ArgumentOutOfRangeException(nameof(maximumConditionNumber));
        if (relativeTolerance <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(relativeTolerance));
        var scale = MaximumAbsoluteElement;
        var determinant = Determinant;
        if (
            !double.IsFinite(scale)
            || scale <= 0.0
            || !double.IsFinite(determinant)
            || Math.Abs(determinant) <= relativeTolerance * scale * scale * scale
        )
        {
            inverse = default;
            conditionNumber = double.PositiveInfinity;
            return false;
        }
        var inverseDeterminant = 1.0 / determinant;
        inverse = new(
            (M22 * M33 - M23 * M32) * inverseDeterminant,
            (M13 * M32 - M12 * M33) * inverseDeterminant,
            (M12 * M23 - M13 * M22) * inverseDeterminant,
            (M23 * M31 - M21 * M33) * inverseDeterminant,
            (M11 * M33 - M13 * M31) * inverseDeterminant,
            (M13 * M21 - M11 * M23) * inverseDeterminant,
            (M21 * M32 - M22 * M31) * inverseDeterminant,
            (M12 * M31 - M11 * M32) * inverseDeterminant,
            (M11 * M22 - M12 * M21) * inverseDeterminant
        );
        conditionNumber = InfinityNorm * inverse.InfinityNorm;
        if (!double.IsFinite(conditionNumber) || conditionNumber > maximumConditionNumber)
        {
            inverse = default;
            return false;
        }
        return true;
    }

    public Matrix3 Inverse(
        double maximumConditionNumber = 1e12,
        double relativeTolerance = 1e-12
    ) =>
        TryInverse(
            out var inverse,
            out var conditionNumber,
            maximumConditionNumber,
            relativeTolerance
        )
            ? inverse
            : throw new InvalidOperationException(
                $"Cannot invert a singular or ill-conditioned matrix (estimated condition number: {conditionNumber:G6})."
            );

    public double MaximumAbsoluteElement =>
        new[]
        {
            Math.Abs(M11),
            Math.Abs(M12),
            Math.Abs(M13),
            Math.Abs(M21),
            Math.Abs(M22),
            Math.Abs(M23),
            Math.Abs(M31),
            Math.Abs(M32),
            Math.Abs(M33),
        }.Max();

    public double InfinityNorm =>
        Math.Max(
            Math.Abs(M11) + Math.Abs(M12) + Math.Abs(M13),
            Math.Max(
                Math.Abs(M21) + Math.Abs(M22) + Math.Abs(M23),
                Math.Abs(M31) + Math.Abs(M32) + Math.Abs(M33)
            )
        );

    public static Vec3 operator *(in Matrix3 matrix, in Vec3 vector) => matrix.Multiply(vector);
}
