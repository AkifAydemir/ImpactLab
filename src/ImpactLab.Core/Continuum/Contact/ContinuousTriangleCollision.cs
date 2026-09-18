using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct TriangleCcdHit(
    bool Hit,
    double TimeOfImpact,
    int TriangleA,
    int TriangleB
);

public static class ContinuousTriangleCollision
{
    public static TriangleCcdHit ConservativeAdvance(
        SurfaceTriangle first,
        SurfaceTriangle second,
        ReadOnlySpan<Vec3> startPositions,
        ReadOnlySpan<Vec3> endPositions,
        double tolerance = 1e-6,
        int iterations = 12
    )
    {
        double lower = 0;
        double upper = 1;

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            var time = (lower + upper) * 0.5;
            var distance = TriangleTriangleDistance.Compute(
                Vec3.Lerp(startPositions[first.A], endPositions[first.A], time),
                Vec3.Lerp(startPositions[first.B], endPositions[first.B], time),
                Vec3.Lerp(startPositions[first.C], endPositions[first.C], time),
                Vec3.Lerp(startPositions[second.A], endPositions[second.A], time),
                Vec3.Lerp(startPositions[second.B], endPositions[second.B], time),
                Vec3.Lerp(startPositions[second.C], endPositions[second.C], time)
            );
            if (distance <= tolerance)
                upper = time;
            else
                lower = time;
        }

        return upper < 1
            ? new(true, upper, first.Id, second.Id)
            : new(false, 1, first.Id, second.Id);
    }
}
