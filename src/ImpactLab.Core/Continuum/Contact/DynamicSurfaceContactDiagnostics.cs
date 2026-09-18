namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct DynamicSurfaceContactDiagnostics(
    int ActivePairs,
    double MaxPenetrationMeters,
    double NormalImpulseNs,
    double FrictionDissipationJ,
    int CcdHits
);
