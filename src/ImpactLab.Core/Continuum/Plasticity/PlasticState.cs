using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Plasticity;

public readonly record struct PlasticState(
    StrainTensor6 PlasticStrain,
    double EquivalentPlasticStrain,
    double YieldStressPa
)
{
    public static PlasticState Zero => new(default, 0, 0);
}
