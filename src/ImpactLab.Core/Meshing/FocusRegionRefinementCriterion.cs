using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Meshing;

public sealed class FocusRegionRefinementCriterion : IAdaptiveRefinementCriterion
{
    public AdaptiveRefinementDecision Evaluate(
        AdaptiveCell cell,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions,
        MeshingSettings settings
    )
    {
        var hits = settings
            .FocusRegions.Where(r =>
                Intersects(cell.Bounds, r.Bounds) && cell.Depth < r.TargetDepth
            )
            .OrderByDescending(r => r.TargetDepth)
            .ToArray();
        return hits.Length == 0
            ? AdaptiveRefinementDecision.No
            : AdaptiveRefinementDecision.Yes(70, $"focus:{hits[0].Id}");
    }

    private static bool Intersects(
        ImpactLab.Core.Mathematics.BoundingBox3 a,
        ImpactLab.Core.Mathematics.BoundingBox3 b
    ) =>
        a.Min.X <= b.Max.X
        && a.Max.X >= b.Min.X
        && a.Min.Y <= b.Max.Y
        && a.Max.Y >= b.Min.Y
        && a.Min.Z <= b.Max.Z
        && a.Max.Z >= b.Min.Z;
}
