namespace ImpactLab.Core.Mathematics;

public readonly struct Vec3 : IEquatable<Vec3>
{
    public static readonly Vec3 Zero = new(0.0, 0.0, 0.0);
    public static readonly Vec3 UnitX = new(1.0, 0.0, 0.0);
    public static readonly Vec3 UnitY = new(0.0, 1.0, 0.0);
    public static readonly Vec3 UnitZ = new(0.0, 0.0, 1.0);

    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => Math.Sqrt(LengthSquared);

    public Vec3 Normalized()
    {
        var length = Length;
        return length <= 1e-12 ? Zero : this / length;
    }

    public static double Dot(in Vec3 a, in Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vec3 Cross(in Vec3 a, in Vec3 b) =>
        new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    public static Vec3 Lerp(in Vec3 a, in Vec3 b, double t) => a + (b - a) * t;

    public static Vec3 Min(in Vec3 a, in Vec3 b) =>
        new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

    public static Vec3 Max(in Vec3 a, in Vec3 b) =>
        new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

    public static Vec3 operator +(in Vec3 a, in Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vec3 operator -(in Vec3 a, in Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vec3 operator -(in Vec3 value) => new(-value.X, -value.Y, -value.Z);

    public static Vec3 operator *(in Vec3 value, double scalar) =>
        new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    public static Vec3 operator *(double scalar, in Vec3 value) => value * scalar;

    public static Vec3 operator /(in Vec3 value, double scalar) =>
        new(value.X / scalar, value.Y / scalar, value.Z / scalar);

    public static bool operator ==(in Vec3 left, in Vec3 right) => left.Equals(right);

    public static bool operator !=(in Vec3 left, in Vec3 right) => !left.Equals(right);

    public bool Equals(Vec3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override bool Equals(object? obj) => obj is Vec3 other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override string ToString() => $"({X:F4}, {Y:F4}, {Z:F4})";
}
