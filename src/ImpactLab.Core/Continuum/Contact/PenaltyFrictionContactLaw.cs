using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public static class PenaltyFrictionContactLaw
{
    public static (Vec3 Force, ContactHistoryState State, double Dissipation) Evaluate(
        Vec3 normal,
        double penetration,
        Vec3 relativeVelocity,
        ContactHistoryState old,
        DynamicSurfaceContactSettings s,
        double dt,
        double time
    )
    {
        var vn = Vec3.Dot(relativeVelocity, normal);
        var fn = Math.Max(0, s.NormalPenaltyNPerM * penetration - s.NormalDampingNsPerM * vn);
        var tangent = relativeVelocity - normal * vn;
        var speed = tangent.Length;
        var mu =
            s.DynamicFriction
            + (s.StaticFriction - s.DynamicFriction) * Math.Exp(-speed / s.SlipRegularizationMps);
        var trial = tangent * (-s.NormalDampingNsPerM * 0.25);
        var limit = mu * fn;
        var ft =
            trial.Length > limit && trial.Length > 1e-30 ? trial * (limit / trial.Length) : trial;
        var dissip = Math.Max(0, -Vec3.Dot(ft, tangent) * dt);
        var next = new ContactHistoryState(
            old.TangentialSlip + tangent * dt,
            old.NormalImpulseNs + fn * dt,
            old.TangentialDissipationJ + dissip,
            time
        );
        return (normal * fn + ft, next, dissip);
    }
}
