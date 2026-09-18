namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct SurfaceContactPair(
    int TriangleA,
    int TriangleB,
    double DistanceMeters,
    bool IsAdjacent
);
