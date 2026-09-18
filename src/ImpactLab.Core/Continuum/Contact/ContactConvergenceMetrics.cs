namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct ContactConvergenceMetrics(
    int ActivePairs,
    double MaxPenetrationMeters,
    double NormalResidualN,
    double TangentialResidualN
);
