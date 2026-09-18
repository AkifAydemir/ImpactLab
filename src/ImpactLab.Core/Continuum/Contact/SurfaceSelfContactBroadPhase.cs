using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class SurfaceSelfContactBroadPhase
{
    public IReadOnlyList<SurfaceContactPair> Find(
        IReadOnlyList<SurfaceTriangle> triangles,
        ReadOnlySpan<Vec3> positions,
        SurfaceSelfContactSettings settings
    )
    {
        var adjacency = new SurfaceAdjacencyMap(triangles);
        var pairs = new List<SurfaceContactPair>();

        for (var firstIndex = 0; firstIndex < triangles.Count; firstIndex++)
        {
            var first = triangles[firstIndex];
            var firstBounds = SurfaceTriangleBounds
                .Of(first, positions)
                .Expand(settings.SearchDistanceMeters);
            for (var secondIndex = firstIndex + 1; secondIndex < triangles.Count; secondIndex++)
            {
                if (adjacency.AreAdjacent(firstIndex, secondIndex))
                    continue;

                var second = triangles[secondIndex];
                var secondBounds = SurfaceTriangleBounds.Of(second, positions);
                if (!firstBounds.Intersects(secondBounds))
                    continue;

                var distance = TriangleTriangleDistance.Compute(
                    positions[first.A],
                    positions[first.B],
                    positions[first.C],
                    positions[second.A],
                    positions[second.B],
                    positions[second.C]
                );
                if (distance <= settings.SearchDistanceMeters)
                    pairs.Add(new(firstIndex, secondIndex, distance, false));
            }
        }

        return pairs;
    }
}
