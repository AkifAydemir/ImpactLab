namespace ImpactLab.Core.Contact;

public readonly record struct ContactStepMetrics(
    int ContactCount,
    double MaxPenetrationMeters,
    double TotalNormalForceN,
    double TotalTangentialForceN,
    double DissipatedFrictionEnergyJ
);
