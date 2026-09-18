namespace ImpactLab.Core.Materials;

public sealed class TabulatedConstitutiveLaw : IConstitutiveLaw
{
    public string Id => "tabulated-profile";

    public ConstitutiveEvaluation Evaluate(MaterialProfile profile, in MaterialStatePoint state)
    {
        state.Validate();
        var curve = state.Strain >= 0 ? profile.TensileCurve : profile.CompressiveCurve;
        if (curve is null)
            return new LegacyBilinearConstitutiveLaw().Evaluate(profile, state);
        var x = Math.Abs(state.Strain);
        var stress = Math.Abs(curve.EvaluateStress(x)) * Math.Sign(state.Strain);
        var eps = Math.Max(1e-8, x * 1e-4);
        var tangent =
            (curve.EvaluateStress(x + eps) - curve.EvaluateStress(Math.Max(0, x - eps)))
            / (2 * eps);
        var fail = profile.Definition.FailureStrain;
        var drive = Math.Clamp(x / Math.Max(fail, 1e-12), 0, 1);
        return new(
            stress * (1 - state.Damage),
            Math.Max(0, tangent),
            Math.Abs(stress * x) * drive * 0.25,
            drive,
            x >= fail
        );
    }
}
