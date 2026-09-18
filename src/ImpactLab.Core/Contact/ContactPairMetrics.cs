namespace ImpactLab.Core.Contact;

public readonly record struct ContactPairMetrics(
    string RigidBodyId,
    string TargetPartId,
    int ContactCount,
    double MaxPenetrationMeters,
    double TotalNormalForceN,
    double TotalTangentialForceN,
    double DissipatedFrictionEnergyJ
);
