namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class CombinedHardeningLaw
{
    public CombinedHardeningLaw(double isotropicModulus, ArmstrongFrederickSettings kinematic)
    {
        IsotropicModulus = isotropicModulus;
        Kinematic = kinematic;
        Kinematic.Validate();
    }

    public double IsotropicModulus { get; }
    public ArmstrongFrederickSettings Kinematic { get; }

    public double YieldRadius(double initial, double eq) => initial + IsotropicModulus * eq;
}
