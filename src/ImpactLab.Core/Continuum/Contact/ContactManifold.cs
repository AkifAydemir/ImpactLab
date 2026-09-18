namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactManifold(
    IReadOnlyList<ContactManifoldPoint> Points,
    int CandidatePairs,
    int ActivePairs,
    double MaxPenetrationMeters
);
