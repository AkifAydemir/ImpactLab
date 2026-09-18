using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Plasticity;

public sealed record ExtendedPlasticState(
    StrainTensor6 PlasticStrain,
    double EquivalentPlasticStrain,
    BackstressTensor6 Backstress,
    double IsotropicHardening,
    double Damage = 0
)
{
    public static ExtendedPlasticState Zero => new(default, 0, default, 0, 0);
}
