using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Plasticity;

public static class StressMath
{
    public static double Mean(StressTensor6 s) => (s.XX + s.YY + s.ZZ) / 3.0;

    public static StressTensor6 Deviator(StressTensor6 s)
    {
        var p = Mean(s);
        return new(s.XX - p, s.YY - p, s.ZZ - p, s.XY, s.YZ, s.ZX);
    }

    public static double J2Norm(StressTensor6 s)
    {
        var d = Deviator(s);
        return Math.Sqrt(
            1.5
                * (
                    d.XX * d.XX
                    + d.YY * d.YY
                    + d.ZZ * d.ZZ
                    + 2 * (d.XY * d.XY + d.YZ * d.YZ + d.ZX * d.ZX)
                )
        );
    }

    public static StressTensor6 Scale(StressTensor6 s, double k) =>
        new(s.XX * k, s.YY * k, s.ZZ * k, s.XY * k, s.YZ * k, s.ZX * k);

    public static StressTensor6 Add(StressTensor6 a, StressTensor6 b) =>
        new(a.XX + b.XX, a.YY + b.YY, a.ZZ + b.ZZ, a.XY + b.XY, a.YZ + b.YZ, a.ZX + b.ZX);
}
