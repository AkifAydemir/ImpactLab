namespace ImpactLab.Core.Mathematics;

public readonly struct BoundingBox3
{
    public BoundingBox3(Vec3 min, Vec3 max)
    {
        Min = min;
        Max = max;
    }

    public Vec3 Min { get; }
    public Vec3 Max { get; }
    public Vec3 Size => Max - Min;
    public Vec3 Center => (Min + Max) * 0.5;

    public bool Contains(in Vec3 point) =>
        point.X >= Min.X
        && point.X <= Max.X
        && point.Y >= Min.Y
        && point.Y <= Max.Y
        && point.Z >= Min.Z
        && point.Z <= Max.Z;

    public bool Intersects(in BoundingBox3 other) =>
        Min.X <= other.Max.X
        && Max.X >= other.Min.X
        && Min.Y <= other.Max.Y
        && Max.Y >= other.Min.Y
        && Min.Z <= other.Max.Z
        && Max.Z >= other.Min.Z;

    public BoundingBox3 Expand(double amount)
    {
        var delta = new Vec3(amount, amount, amount);
        return new BoundingBox3(Min - delta, Max + delta);
    }

    public static BoundingBox3 FromPoints(IEnumerable<Vec3> points)
    {
        using var e = points.GetEnumerator();
        if (!e.MoveNext())
            throw new ArgumentException("At least one point is required.", nameof(points));
        var min = e.Current;
        var max = e.Current;
        while (e.MoveNext())
        {
            min = Vec3.Min(min, e.Current);
            max = Vec3.Max(max, e.Current);
        }
        return new BoundingBox3(min, max);
    }
}
