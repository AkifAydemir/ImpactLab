using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Meshing;

public sealed class MaterialInterfaceRefinementCriterion : IAdaptiveRefinementCriterion
{
    public AdaptiveRefinementDecision Evaluate(
        AdaptiveCell cell,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions,
        MeshingSettings settings
    )
    {
        if (regions.Count == 0)
            return AdaptiveRefinementDecision.No;
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in Sample(cell))
        {
            if (!geometry.Contains(p))
                continue;
            ids.Add(Resolve(p, baseMaterial, regions).Id);
        }
        return ids.Count > 1
            ? AdaptiveRefinementDecision.Yes(80, "material interface")
            : AdaptiveRefinementDecision.No;
    }

    private static IEnumerable<ImpactLab.Core.Mathematics.Vec3> Sample(AdaptiveCell c)
    {
        yield return c.Center;
        foreach (var p in c.Split())
            yield return p.Center;
    }

    private static MaterialDefinition Resolve(
        in ImpactLab.Core.Mathematics.Vec3 p,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions
    )
    {
        foreach (var r in regions.OrderByDescending(x => x.Priority))
            if (r.Geometry.Contains(p))
                return r.Material;
        return baseMaterial;
    }
}
