using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Plasticity;

public static class StrainMath
{
    public static StrainTensor6 Add(StrainTensor6 a, StrainTensor6 b) =>
        new(a.XX + b.XX, a.YY + b.YY, a.ZZ + b.ZZ, a.XY + b.XY, a.YZ + b.YZ, a.ZX + b.ZX);

    public static StrainTensor6 Subtract(StrainTensor6 a, StrainTensor6 b) =>
        new(a.XX - b.XX, a.YY - b.YY, a.ZZ - b.ZZ, a.XY - b.XY, a.YZ - b.YZ, a.ZX - b.ZX);

    public static StrainTensor6 Scale(StrainTensor6 a, double k) =>
        new(a.XX * k, a.YY * k, a.ZZ * k, a.XY * k, a.YZ * k, a.ZX * k);
}
