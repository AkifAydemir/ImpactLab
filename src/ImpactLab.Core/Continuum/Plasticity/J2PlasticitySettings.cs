namespace ImpactLab.Core.Continuum.Plasticity;

public sealed record J2PlasticitySettings(
    double InitialYieldStressPa,
    double HardeningModulusPa = 0.0,
    double MaxEquivalentPlasticStrain = 10.0,
    HardeningKind Hardening = HardeningKind.LinearIsotropic
)
{
    public void Validate()
    {
        if (InitialYieldStressPa <= 0 || HardeningModulusPa < 0 || MaxEquivalentPlasticStrain <= 0)
            throw new InvalidOperationException("Invalid J2 plasticity settings.");
    }
}
