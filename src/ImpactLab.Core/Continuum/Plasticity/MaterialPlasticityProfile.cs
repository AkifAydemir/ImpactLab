using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Plasticity;

public sealed record MaterialPlasticityProfile(
    string MaterialId,
    J2PlasticitySettings Settings,
    IReadOnlyList<PiecewiseHardeningPoint>? HardeningCurve = null
)
{
    public IHardeningLaw CreateLaw() =>
        Settings.Hardening switch
        {
            HardeningKind.PerfectPlastic => new PerfectPlasticHardening(
                Settings.InitialYieldStressPa
            ),
            HardeningKind.PiecewiseIsotropic when HardeningCurve is { Count: >= 2 } =>
                new PiecewiseIsotropicHardening(HardeningCurve),
            _ => new LinearIsotropicHardening(
                Settings.InitialYieldStressPa,
                Settings.HardeningModulusPa
            ),
        };
}
