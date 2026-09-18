using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Authoring;

public sealed class AuthoringSelectionService
{
    public GeometryHit? Pick(IEnumerable<(string Id, GeometrySpec Geometry)> parts, Ray3 ray)
    {
        GeometryHit? best = null;
        foreach (var p in parts)
        {
            var hit = GeometryHitTester.Hit(p.Id, p.Geometry, ray);
            if (hit is not null && (best is null || hit.Value.Distance < best.Value.Distance))
                best = hit;
        }
        return best;
    }
}
