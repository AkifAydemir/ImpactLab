using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearTransientFrame(
    double TimeSeconds,
    Vec3[] Displacement,
    Vec3[] Velocity,
    Vec3[] Acceleration,
    double[] EquivalentPlasticStrain
)
{
    public static NonlinearTransientFrame Capture(
        TetrahedralMesh mesh,
        double time,
        double[] u,
        double[] v,
        double[] a,
        IReadOnlyList<PlasticState> p
    ) =>
        new(time, ToVec(u), ToVec(v), ToVec(a), p.Select(x => x.EquivalentPlasticStrain).ToArray());

    private static Vec3[] ToVec(double[] x)
    {
        var r = new Vec3[x.Length / 3];
        for (var i = 0; i < r.Length; i++)
            r[i] = new(x[i * 3], x[i * 3 + 1], x[i * 3 + 2]);
        return r;
    }
}
