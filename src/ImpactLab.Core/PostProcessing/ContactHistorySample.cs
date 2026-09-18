namespace ImpactLab.Core.PostProcessing;

public readonly record struct ContactHistorySample(
    double TimeSeconds,
    int ContactCount,
    double MaxPenetrationMeters,
    double NormalForceN,
    double TangentialForceN,
    double FrictionEnergyJ
);
