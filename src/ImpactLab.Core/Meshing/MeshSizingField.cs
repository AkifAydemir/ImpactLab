using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed class MeshSizingField
{
    public MeshSizingField(
        double baseCellSizeMeters,
        IEnumerable<LocalMeshSizingRegion>? regions = null
    )
    {
        if (baseCellSizeMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseCellSizeMeters));
        BaseCellSizeMeters = baseCellSizeMeters;
        Regions = (regions ?? []).OrderByDescending(x => x.Priority).ToArray();
    }

    public double BaseCellSizeMeters { get; }
    public IReadOnlyList<LocalMeshSizingRegion> Regions { get; }

    public double At(in Vec3 position)
    {
        var size = BaseCellSizeMeters;
        foreach (var r in Regions)
            if (r.Bounds.Contains(position))
                size = Math.Min(size, r.TargetCellSizeMeters);
        return size;
    }
}
