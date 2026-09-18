namespace ImpactLab.Core.Continuum.Plasticity;

public readonly record struct BackstressTensor6(
    double Xx,
    double Yy,
    double Zz,
    double Xy,
    double Yz,
    double Zx
)
{
    public static BackstressTensor6 Zero => default;
}
