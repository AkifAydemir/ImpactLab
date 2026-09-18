namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class NeoHookeanLaw : IFiniteStrainConstitutiveLaw
{
    public string Id => "neo-hookean";

    public FiniteStrainMaterialUpdate Update(FiniteStrainMaterialContext c)
    {
        var E = c.Material.YoungModulusPa;
        var nu = c.Material.PoissonRatio;
        var mu = E / (2 * (1 + nu));
        var lambda = E * nu / ((1 + nu) * (1 - 2 * nu));
        var b = c.Kinematics.F.LeftCauchyGreen;
        var j = c.Kinematics.Jacobian;
        var tau = (b - Matrix3.Identity) * mu + Matrix3.Identity * (lambda * Math.Log(j));
        var state = c.Previous with
        {
            F = c.Kinematics.F,
            CauchyStress = tau * (1.0 / j),
            TemperatureKelvin = c.TemperatureKelvin,
        };
        return new(
            state,
            tau,
            IsotropicTangent6.Build(lambda, mu),
            0.5 * mu * (b.Trace - 3) - mu * Math.Log(j) + 0.5 * lambda * Math.Log(j) * Math.Log(j),
            0,
            true
        );
    }
}
