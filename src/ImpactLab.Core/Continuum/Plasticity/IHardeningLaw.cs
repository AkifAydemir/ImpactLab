namespace ImpactLab.Core.Continuum.Plasticity;

public interface IHardeningLaw
{
    double YieldStress(double equivalentPlasticStrain);
    double Tangent(double equivalentPlasticStrain);
}
