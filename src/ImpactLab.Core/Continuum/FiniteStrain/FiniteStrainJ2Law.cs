namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class FiniteStrainJ2Law : IFiniteStrainConstitutiveLaw
{
    private readonly FiniteStrainJ2Settings _s;
    private readonly NeoHookeanLaw _elastic = new();
    public string Id => "finite-j2";

    public FiniteStrainJ2Law(FiniteStrainJ2Settings? settings = null) => _s = settings ?? new();

    public FiniteStrainMaterialUpdate Update(FiniteStrainMaterialContext c)
    {
        var trial = _elastic.Update(c);
        var dev = Matrix3Math.Deviator(trial.KirchhoffStress);
        var q = Math.Sqrt(1.5) * Matrix3Math.Frobenius(dev);
        var yield = _s.YieldStressPa + _s.IsotropicHardeningPa * c.Previous.EquivalentPlasticStrain;
        if (q <= yield)
            return trial;
        var mu = c.Material.YoungModulusPa / (2 * (1 + c.Material.PoissonRatio));
        var dg = (q - yield) / (3 * mu + _s.IsotropicHardeningPa);
        var scale = Math.Max(0, 1 - 3 * mu * dg / Math.Max(q, 1e-30));
        var tau = Matrix3.Identity * (trial.KirchhoffStress.Trace / 3.0) + dev * scale;
        var eq = c.Previous.EquivalentPlasticStrain + dg;
        var damage = Math.Clamp(
            (eq - _s.DamageOnsetPlasticStrain)
                / Math.Max(_s.FailurePlasticStrain - _s.DamageOnsetPlasticStrain, 1e-12),
            0,
            1
        );
        var state = trial.State with
        {
            CauchyStress = tau * (1.0 / c.Kinematics.Jacobian),
            EquivalentPlasticStrain = eq,
            Damage = damage,
        };
        return trial with
        {
            State = state,
            KirchhoffStress = tau,
            PlasticDissipationJ = yield * dg,
            Converged = true,
        };
    }
}
