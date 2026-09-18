using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class ContactManifoldBuilder
{
    public ContactManifold Build(
        TetrahedralMesh mesh,
        IReadOnlyList<SurfaceTriangle> slave,
        IReadOnlyList<SurfaceTriangle> master,
        ReadOnlySpan<Vec3> position,
        double search
    )
    {
        var pts = new List<ContactManifoldPoint>();
        var candidates = 0;
        foreach (var s in slave)
        foreach (var m in master)
        {
            if (s.Id == m.Id)
                continue;
            candidates++;
            var sc = (position[s.A] + position[s.B] + position[s.C]) / 3.0;
            var mc = (position[m.A] + position[m.B] + position[m.C]) / 3.0;
            var n = Vec3.Cross(position[m.B] - position[m.A], position[m.C] - position[m.A])
                .Normalized();
            var gap = Vec3.Dot(sc - mc, n);
            if (gap < search)
                pts.Add(
                    new(
                        new(s.Id, m.Id, 0),
                        sc,
                        mc,
                        n,
                        gap,
                        TriangleArea(position[m.A], position[m.B], position[m.C]),
                        0,
                        0
                    )
                );
        }
        return new(
            pts,
            candidates,
            pts.Count,
            pts.Count == 0 ? 0 : Math.Max(0, -pts.Min(x => x.GapMeters))
        );
    }

    static double TriangleArea(Vec3 a, Vec3 b, Vec3 c) => 0.5 * Vec3.Cross(b - a, c - a).Length;
}
