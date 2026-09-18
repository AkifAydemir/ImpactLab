namespace ImpactLab.Core.Continuum.Contact;

public sealed record LargeSlidingContactSettings(
    double SearchRadiusMeters = 0.002,
    int BvhRebuildStride = 1,
    bool EnableEdgeEdge = true,
    bool EnableCcd = true,
    double CcdTolerance = 1e-6
);
