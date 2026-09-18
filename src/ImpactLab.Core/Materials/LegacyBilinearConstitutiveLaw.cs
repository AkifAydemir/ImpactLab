namespace ImpactLab.Core.Materials;

public sealed class LegacyBilinearConstitutiveLaw : IConstitutiveLaw
{
    public string Id => "legacy-bilinear";

    public ConstitutiveEvaluation Evaluate(MaterialProfile profile, in MaterialStatePoint state)
    {
        state.Validate();
        var m = profile.Definition;
        var abs = Math.Abs(state.Strain);
        var sign = Math.Sign(state.Strain);
        double stress,
            tangent;
        if (abs <= m.YieldStrain)
        {
            stress = m.YoungModulusPa * state.Strain;
            tangent = m.YoungModulusPa;
        }
        else
        {
            var yield = m.YoungModulusPa * m.YieldStrain;
            stress = sign * (yield + m.TangentModulusPa * (abs - m.YieldStrain));
            tangent = m.TangentModulusPa;
        }
        var drive =
            m.FailureStrain <= m.YieldStrain
                ? 1.0
                : Math.Clamp((abs - m.YieldStrain) / (m.FailureStrain - m.YieldStrain), 0, 1);
        stress *= 1.0 - Math.Clamp(state.Damage, 0, 1);
        return new(
            stress,
            tangent,
            Math.Max(0, abs * m.YoungModulusPa * drive * 0.5),
            drive,
            abs >= m.FailureStrain
        );
    }
}
