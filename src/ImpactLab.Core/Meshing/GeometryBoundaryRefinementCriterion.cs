using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed class GeometryBoundaryRefinementCriterion : IAdaptiveRefinementCriterion
{
    public AdaptiveRefinementDecision Evaluate(
        AdaptiveCell cell,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions,
        MeshingSettings settings
    )
    {
        var inside = 0;
        foreach (var p in Corners(cell.Bounds))
            if (geometry.Contains(p))
                inside++;
        if (geometry.Contains(cell.Center))
            inside++;
        if (inside > 0 && inside < 9)
            return AdaptiveRefinementDecision.Yes(100, "geometry boundary");
        var half = cell.CharacteristicSize * 0.52;
        var probes = new[]
        {
            cell.Center + new Vec3(half, 0, 0),
            cell.Center - new Vec3(half, 0, 0),
            cell.Center + new Vec3(0, half, 0),
            cell.Center - new Vec3(0, half, 0),
            cell.Center + new Vec3(0, 0, half),
            cell.Center - new Vec3(0, 0, half),
        };
        if (probes.Any(x => geometry.Contains(x) != geometry.Contains(cell.Center)))
            return AdaptiveRefinementDecision.Yes(90, "near boundary");
        return AdaptiveRefinementDecision.No;
    }

    private static IEnumerable<Vec3> Corners(BoundingBox3 b)
    {
        foreach (var x in new[] { b.Min.X, b.Max.X })
        foreach (var y in new[] { b.Min.Y, b.Max.Y })
        foreach (var z in new[] { b.Min.Z, b.Max.Z })
            yield return new Vec3(x, y, z);
    }
}
