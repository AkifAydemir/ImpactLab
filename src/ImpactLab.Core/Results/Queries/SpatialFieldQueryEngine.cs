using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Results.Queries;

public sealed class SpatialFieldQueryEngine
{
    public SpatialFieldQueryResult Query(
        string id,
        int frame,
        IReadOnlyList<Vec3> positions,
        double[] values,
        SpatialQueryRegion r
    )
    {
        var idx = new List<int>();
        for (var i = 0; i < positions.Count; i++)
        {
            var p = positions[i];
            if (
                p.X >= r.Min.X
                && p.Y >= r.Min.Y
                && p.Z >= r.Min.Z
                && p.X <= r.Max.X
                && p.Y <= r.Max.Y
                && p.Z <= r.Max.Z
            )
                idx.Add(i);
        }
        var v = idx.Select(i => values[Math.Min(i, values.Length - 1)]).ToArray();
        if (v.Length == 0)
            return new(id, frame, 0, 0, 0, 0, 0, idx);
        return new(
            id,
            frame,
            v.Length,
            v.Min(),
            v.Max(),
            v.Average(),
            Math.Sqrt(v.Average(x => x * x)),
            idx
        );
    }
}
