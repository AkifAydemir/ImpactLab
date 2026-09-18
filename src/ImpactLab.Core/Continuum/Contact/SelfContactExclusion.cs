namespace ImpactLab.Core.Continuum.Contact;

public static class SelfContactExclusion
{
    public static bool SharesNode(
        ImpactLab.Core.Continuum.SurfaceTriangle a,
        ImpactLab.Core.Continuum.SurfaceTriangle b
    ) =>
        a.A == b.A
        || a.A == b.B
        || a.A == b.C
        || a.B == b.A
        || a.B == b.B
        || a.B == b.C
        || a.C == b.A
        || a.C == b.B
        || a.C == b.C;
}
