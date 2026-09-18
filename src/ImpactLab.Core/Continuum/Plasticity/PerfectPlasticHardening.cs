namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class PerfectPlasticHardening(double yield) : IHardeningLaw
{
    public double YieldStress(double e) => yield;

    public double Tangent(double e) => 0;
}
