using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class EdgeEdgeDistance
{
    public static EdgeEdgeContactCandidate Closest(EdgeSegment a, EdgeSegment b)
    {
        var u = a.P1 - a.P0;
        var v = b.P1 - b.P0;
        var w = a.P0 - b.P0;
        var aa = Vec3.Dot(u, u);
        var bb = Vec3.Dot(u, v);
        var cc = Vec3.Dot(v, v);
        var dd = Vec3.Dot(u, w);
        var ee = Vec3.Dot(v, w);
        var den = aa * cc - bb * bb;
        var s = Math.Clamp((bb * ee - cc * dd) / Math.Max(den, 1e-30), 0, 1);
        var t = Math.Clamp((aa * ee - bb * dd) / Math.Max(den, 1e-30), 0, 1);
        var pa = a.P0 + u * s;
        var pb = b.P0 + v * t;
        return new(a.Id, b.Id, pa, pb, (pa - pb).Length, s, t);
    }
}
