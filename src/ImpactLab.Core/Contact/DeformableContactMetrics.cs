namespace ImpactLab.Core.Contact;

public readonly record struct DeformableContactMetrics(
    string PartA,
    string PartB,
    int ContactCount,
    double MaxOverlapMeters,
    double TotalNormalForceN
);
