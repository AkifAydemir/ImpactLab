using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record EdgeEdgeContactCandidate(
    int EdgeA,
    int EdgeB,
    Vec3 PointA,
    Vec3 PointB,
    double DistanceMeters,
    double ParameterA,
    double ParameterB
);
