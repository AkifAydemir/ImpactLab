using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactHistoryState(
    Vec3 TangentialSlip,
    double NormalImpulseNs,
    double TangentialDissipationJ,
    double LastTimeSeconds
);
