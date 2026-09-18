using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactManifoldPoint(
    ContactConstraintKey Key,
    Vec3 SlavePoint,
    Vec3 MasterPoint,
    Vec3 Normal,
    double GapMeters,
    double AreaWeight,
    double RelativeNormalVelocity,
    double RelativeTangentialSpeed
);
