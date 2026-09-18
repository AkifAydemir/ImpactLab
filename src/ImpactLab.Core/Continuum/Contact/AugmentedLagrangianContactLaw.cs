using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class AugmentedLagrangianContactLaw
{
    private readonly AugmentedContactSettings _s;

    public AugmentedLagrangianContactLaw(AugmentedContactSettings? settings = null) =>
        _s = settings ?? new();

    public (double normal, Vec3 tangent, ContactMultiplierState next) Evaluate(
        ContactManifoldPoint p,
        ContactMultiplierState old,
        double penalty,
        Vec3 tangentialVelocity,
        double dt
    )
    {
        var trial = Math.Max(0, old.NormalMultiplierN - penalty * p.GapMeters);
        var vt = tangentialVelocity.Length;
        var tTrial =
            new Vec3(old.TangentialX, old.TangentialY, 0) - tangentialVelocity * (penalty * dt);
        var limit = _s.FrictionCoefficient * trial;
        var mag = tTrial.Length;
        var sticking = vt < _s.StickToleranceMps && mag <= limit;
        var t = sticking ? tTrial : (mag <= 1e-20 ? Vec3.Zero : tTrial * (limit / mag));
        return (trial, t, new(trial, t.X, t.Y, sticking, p.GapMeters, old.Age + 1));
    }
}
