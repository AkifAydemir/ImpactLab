namespace ImpactLab.Core.Continuum.Plasticity;

public sealed record ArmstrongFrederickSettings(double C = 0, double Gamma = 0)
{
    public void Validate()
    {
        if (C < 0 || Gamma < 0)
            throw new InvalidOperationException("Invalid kinematic hardening parameters.");
    }
}
