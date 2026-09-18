using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed record AdaptiveCell(long Key, BoundingBox3 Bounds, int Depth)
{
    public Vec3 Center => Bounds.Center;
    public Vec3 Size => Bounds.Size;
    public double CharacteristicSize => Math.Max(Size.X, Math.Max(Size.Y, Size.Z));
    public double Volume => Size.X * Size.Y * Size.Z;

    public IEnumerable<AdaptiveCell> Split()
    {
        var min = Bounds.Min;
        var max = Bounds.Max;
        var mid = Bounds.Center;
        long n = Key << 3;
        for (var z = 0; z < 2; z++)
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
        {
            var a = new Vec3(
                x == 0 ? min.X : mid.X,
                y == 0 ? min.Y : mid.Y,
                z == 0 ? min.Z : mid.Z
            );
            var b = new Vec3(
                x == 0 ? mid.X : max.X,
                y == 0 ? mid.Y : max.Y,
                z == 0 ? mid.Z : max.Z
            );
            yield return new AdaptiveCell(n++, new BoundingBox3(a, b), Depth + 1);
        }
    }
}
