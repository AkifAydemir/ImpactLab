using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Spatial;

public sealed class SpatialHash3
{
    private readonly Dictionary<(int X, int Y, int Z), List<int>> _cells = [];

    public SpatialHash3(double cellSize)
    {
        if (cellSize <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        CellSize = cellSize;
    }

    public double CellSize { get; }

    public void Clear() => _cells.Clear();

    public void Insert(int id, in Vec3 point)
    {
        var key = Key(point);
        if (!_cells.TryGetValue(key, out var bucket))
            _cells[key] = bucket = [];
        bucket.Add(id);
    }

    public void Query(in Vec3 point, double radius, List<int> results)
    {
        results.Clear();
        var min = Key(point - new Vec3(radius, radius, radius));
        var max = Key(point + new Vec3(radius, radius, radius));
        for (var x = min.X; x <= max.X; x++)
        for (var y = min.Y; y <= max.Y; y++)
        for (var z = min.Z; z <= max.Z; z++)
            if (_cells.TryGetValue((x, y, z), out var bucket))
                results.AddRange(bucket);
    }

    private (int X, int Y, int Z) Key(in Vec3 p) =>
        (
            (int)Math.Floor(p.X / CellSize),
            (int)Math.Floor(p.Y / CellSize),
            (int)Math.Floor(p.Z / CellSize)
        );
}
