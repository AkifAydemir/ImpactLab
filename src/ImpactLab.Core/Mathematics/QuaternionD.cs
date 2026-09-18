namespace ImpactLab.Core.Mathematics;

public readonly struct QuaternionD : IEquatable<QuaternionD>
{
    public static readonly QuaternionD Identity = new(0.0, 0.0, 0.0, 1.0);

    public QuaternionD(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public double W { get; }
    public double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    public QuaternionD Normalized()
    {
        var length = Math.Sqrt(LengthSquared);
        return length <= 1e-15
            ? Identity
            : new QuaternionD(X / length, Y / length, Z / length, W / length);
    }

    public QuaternionD Conjugate() => new(-X, -Y, -Z, W);

    public Vec3 Rotate(in Vec3 value)
    {
        var q = Normalized();
        var v = new QuaternionD(value.X, value.Y, value.Z, 0.0);
        var rotated = q * v * q.Conjugate();
        return new Vec3(rotated.X, rotated.Y, rotated.Z);
    }

    public Vec3 InverseRotate(in Vec3 value) => Conjugate().Normalized().Rotate(value);

    public static QuaternionD FromAxisAngle(in Vec3 axis, double angleRadians)
    {
        var n = axis.Normalized();
        if (n.LengthSquared <= 1e-15)
            return Identity;
        var half = angleRadians * 0.5;
        var s = Math.Sin(half);
        return new QuaternionD(n.X * s, n.Y * s, n.Z * s, Math.Cos(half)).Normalized();
    }

    public static QuaternionD FromAngularVelocity(in Vec3 angularVelocityRadPerSec, double dt)
    {
        var speed = angularVelocityRadPerSec.Length;
        return speed <= 1e-15
            ? Identity
            : FromAxisAngle(angularVelocityRadPerSec / speed, speed * dt);
    }

    public static QuaternionD operator *(in QuaternionD a, in QuaternionD b) =>
        new(
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
        );

    public static bool operator ==(in QuaternionD left, in QuaternionD right) => left.Equals(right);

    public static bool operator !=(in QuaternionD left, in QuaternionD right) =>
        !left.Equals(right);

    public bool Equals(QuaternionD other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    public override bool Equals(object? obj) => obj is QuaternionD other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public override string ToString() => $"({X:F4}, {Y:F4}, {Z:F4}, {W:F4})";
}
