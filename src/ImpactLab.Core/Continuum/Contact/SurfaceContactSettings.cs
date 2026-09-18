namespace ImpactLab.Core.Continuum.Contact;

public sealed record SurfaceContactSettings(
    double SearchRadiusMeters = 0.01,
    double PenaltyStiffnessNPerM = 5e8,
    double DampingNsPerM = 5e3,
    double FrictionCoefficient = 0.25
);
