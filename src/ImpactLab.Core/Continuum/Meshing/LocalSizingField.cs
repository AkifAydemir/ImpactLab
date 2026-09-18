using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing;

public sealed class LocalSizingField
{
    private readonly List<(BoundingBox3 Box, double Size)> _regions = [];

    public void Add(BoundingBox3 box, double size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        _regions.Add((box, size));
    }

    public double Evaluate(Vec3 p, double fallback) =>
        _regions.Where(x => x.Box.Contains(p)).Select(x => x.Size).DefaultIfEmpty(fallback).Min();
}
