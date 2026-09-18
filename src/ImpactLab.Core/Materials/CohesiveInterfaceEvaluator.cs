namespace ImpactLab.Core.Materials;

public static class CohesiveInterfaceEvaluator
{
    public static CohesiveEvaluation Evaluate(
        CohesiveInterfaceLaw law,
        CohesiveInterfaceState state,
        double normalSeparation,
        double shearSeparation,
        double area,
        double dt
    )
    {
        law.Validate();
        var eq = Math.Pow(
            Math.Pow(Math.Max(0, normalSeparation), law.MixedModeExponent)
                + Math.Pow(Math.Abs(shearSeparation), law.MixedModeExponent),
            1.0 / law.MixedModeExponent
        );
        state.MaximumEquivalentSeparationMeters = Math.Max(
            state.MaximumEquivalentSeparationMeters,
            eq
        );
        var d =
            eq <= law.DamageOnsetSeparationMeters
                ? state.Damage
                : Math.Max(
                    state.Damage,
                    Math.Clamp(
                        (eq - law.DamageOnsetSeparationMeters)
                            / (law.FailureSeparationMeters - law.DamageOnsetSeparationMeters),
                        0,
                        1
                    )
                );
        var degrade = (1 - d) * (1 - law.ResidualTractionFraction) + law.ResidualTractionFraction;
        var tn = law.NormalStiffnessPaPerM * normalSeparation * degrade;
        var ts = law.ShearStiffnessPaPerM * shearSeparation * degrade;
        var dissip =
            Math.Max(0, (d - state.Damage))
            * 0.5
            * (Math.Abs(tn * normalSeparation) + Math.Abs(ts * shearSeparation));
        state.Damage = d;
        state.DissipatedEnergyJ += dissip * Math.Max(area, 0);
        return new(tn, ts, d, dissip, d >= 0.999999);
    }
}
