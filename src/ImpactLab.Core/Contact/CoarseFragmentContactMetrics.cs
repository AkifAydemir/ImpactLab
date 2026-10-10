namespace ImpactLab.Core.Contact;

public readonly record struct CoarseFragmentContactMetrics(
    string BodyA,
    string BodyB,
    int ContactCount,
    double MaxOverlapMeters,
    double TotalNormalForceN,
    double TotalTangentialForceN,
    double DissipatedFrictionEnergyJ,
    double StoredNormalPenaltyEnergyJ,
    double StoredTangentialPenaltyEnergyJ,
    double DissipatedNormalDampingEnergyJ,
    double CriticalTimeStepSeconds,
    double MomentumBalanceResidualN
);
