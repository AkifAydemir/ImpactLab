namespace ImpactLab.Core.Simulation;

public readonly record struct SimulationTelemetrySample(
    double TimeSeconds,
    double KineticEnergyJ,
    double RotationalKineticEnergyJ,
    double ElasticEnergyJ,
    double MaxDisplacementMeters,
    double MaxNodeDamage,
    int BrokenSpringCount,
    int ContactCount,
    double MaxContactPenetrationMeters,
    double TotalNormalContactForceN,
    double TotalTangentialContactForceN,
    double FrictionEnergyJ,
    int DeformableContactCount
);
