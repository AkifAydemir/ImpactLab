namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class LinearIsotropicHardening(double yield, double modulus) : IHardeningLaw
{
    public double YieldStress(double e) => yield + modulus * Math.Max(0, e);

    public double Tangent(double e) => modulus;
}
