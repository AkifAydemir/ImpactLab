using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Contact;

public static class ContactJacobianApproximation
{
    public static void AddNormalPenalty(SparseTripletBuilder b, SurfaceTriangle t, double k)
    {
        foreach (var n in t.Nodes())
            for (var a = 0; a < 3; a++)
                b.Add(n * 3 + a, n * 3 + a, k / 3.0);
    }
}
