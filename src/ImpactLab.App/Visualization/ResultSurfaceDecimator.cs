using ImpactLab.Core.Continuum;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Visualization;

public static class ResultSurfaceDecimator
{
    public static DecimatedSurface Decimate(
        ReadOnlySpan<Vec3> positions,
        IReadOnlyList<SurfaceTriangle> triangles,
        MeshDecimationSettings settings
    )
    {
        if (triangles.Count <= settings.TargetTriangles)
            return new(triangles, triangles.Count, 1);
        var target = Math.Max(1, settings.TargetTriangles);
        var positionArray = positions.ToArray();
        var ranked = triangles
            .Select(
                (triangle, index) =>
                    new
                    {
                        Triangle = triangle,
                        Index = index,
                        Area = Area(triangle, positionArray),
                    }
            )
            .ToArray();
        var selected = new Dictionary<int, SurfaceTriangle>();
        if (settings.PreservePartInterfaces)
            foreach (
                var g in ranked.GroupBy(x => x.Triangle.PartId, StringComparer.OrdinalIgnoreCase)
            )
            foreach (
                var x in g.OrderByDescending(x => x.Area)
                    .ThenBy(x => x.Index)
                    .Take(Math.Min(8, g.Count()))
            )
                selected[x.Index] = x.Triangle;
        foreach (var x in ranked.OrderByDescending(x => x.Area).ThenBy(x => x.Index))
        {
            if (selected.Count >= target)
                break;
            selected[x.Index] = x.Triangle;
        }
        var result = selected.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        return new(result, triangles.Count, (double)result.Length / triangles.Count);
    }

    private static double Area(SurfaceTriangle t, ReadOnlySpan<Vec3> p) =>
        0.5 * Vec3.Cross(p[t.B] - p[t.A], p[t.C] - p[t.A]).Length;
}
