using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class SelfContactForceAssembler
{
    public int Apply(
        IReadOnlyList<SurfaceTriangle> tri,
        ReadOnlySpan<Vec3> pos,
        ReadOnlySpan<Vec3> vel,
        IReadOnlyList<SurfaceContactPair> pairs,
        SurfaceSelfContactSettings s,
        Span<Vec3> force
    )
    {
        var count = 0;
        foreach (var pair in pairs)
        {
            var a = tri[pair.TriangleA];
            var b = tri[pair.TriangleB];
            var ca = (pos[a.A] + pos[a.B] + pos[a.C]) / 3;
            var cb = (pos[b.A] + pos[b.B] + pos[b.C]) / 3;
            var d = cb - ca;
            var len = d.Length;
            if (len < 1e-12)
                continue;
            var n = d / len;
            var penetration = s.SearchDistanceMeters - pair.DistanceMeters;
            if (penetration <= 0)
                continue;
            var va = (vel[a.A] + vel[a.B] + vel[a.C]) / 3;
            var vb = (vel[b.A] + vel[b.B] + vel[b.C]) / 3;
            var vn = Vec3.Dot(vb - va, n);
            var mag = Math.Max(0, s.PenaltyNPerM * penetration - s.DampingNsPerM * vn);
            var f = n * mag;
            foreach (var id in a.Nodes())
                force[id] -= f / 3;
            foreach (var id in b.Nodes())
                force[id] += f / 3;
            count++;
        }
        return count;
    }
}
