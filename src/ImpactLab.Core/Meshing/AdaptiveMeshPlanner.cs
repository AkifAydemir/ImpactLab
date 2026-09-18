using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed class AdaptiveMeshPlanner
{
    private readonly IReadOnlyList<IAdaptiveRefinementCriterion> _criteria;

    public AdaptiveMeshPlanner(IEnumerable<IAdaptiveRefinementCriterion>? criteria = null) =>
        _criteria = (
            criteria
            ??
            [
                new GeometryBoundaryRefinementCriterion(),
                new MaterialInterfaceRefinementCriterion(),
                new FocusRegionRefinementCriterion(),
            ]
        ).ToArray();

    public AdaptiveMeshPlan Plan(
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion>? regions,
        MeshingSettings settings
    )
    {
        settings.Validate();
        var rs = (regions ?? []).OrderByDescending(x => x.Priority).ToArray();
        var root = RootCell(geometry.Bounds, settings.BaseCellSizeMeters);
        var pending = new Stack<AdaptiveCell>();
        pending.Push(root);
        var leaves = new List<AdaptiveCell>();
        var reasons = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var maxDepth = 0;
        while (pending.Count > 0)
        {
            var c = pending.Pop();
            if (!IntersectsGeometry(c, geometry))
                continue;
            var decision = Decide(c, geometry, baseMaterial, rs, settings);
            var can =
                c.Depth < settings.MaximumRefinementDepth
                && c.CharacteristicSize * 0.5 >= settings.MinimumCellSizeMeters * 0.999;
            if (decision.Refine && can)
            {
                reasons[decision.Reason] = reasons.GetValueOrDefault(decision.Reason) + 1;
                foreach (var child in c.Split())
                    pending.Push(child);
                maxDepth = Math.Max(maxDepth, c.Depth + 1);
            }
            else
                leaves.Add(c);
        }
        if (leaves.Count == 0)
            throw new InvalidOperationException("Adaptive mesher produced no occupied cells.");
        return new AdaptiveMeshPlan(
            leaves,
            maxDepth,
            leaves.Min(x => x.CharacteristicSize),
            leaves.Max(x => x.CharacteristicSize),
            reasons
        );
    }

    private AdaptiveRefinementDecision Decide(
        AdaptiveCell c,
        GeometrySpec g,
        MaterialDefinition m,
        IReadOnlyList<MaterialRegion> r,
        MeshingSettings s
    )
    {
        if (c.CharacteristicSize > s.BaseCellSizeMeters * 1.001)
            return AdaptiveRefinementDecision.Yes(200, "base resolution");
        return _criteria
            .Select(x => x.Evaluate(c, g, m, r, s))
            .Where(x => x.Refine)
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault();
    }

    private static bool IntersectsGeometry(AdaptiveCell c, GeometrySpec g)
    {
        if (g.Contains(c.Center))
            return true;
        foreach (var p in c.Split())
            if (g.Contains(p.Center))
                return true;
        return false;
    }

    private static AdaptiveCell RootCell(BoundingBox3 bounds, double baseCell)
    {
        var size = bounds.Size;
        var side = Math.Max(size.X, Math.Max(size.Y, size.Z));
        var cells = Math.Max(1, Math.Ceiling(side / baseCell));
        var pow = Math.Pow(2, Math.Ceiling(Math.Log(cells, 2)));
        side = pow * baseCell;
        var h = new Vec3(side, side, side) * 0.5;
        return new AdaptiveCell(1, new BoundingBox3(bounds.Center - h, bounds.Center + h), 0);
    }
}
