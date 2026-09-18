namespace ImpactLab.Core.Diagnostics;

public readonly record struct NumericalHealthSample(
    int Step,
    double TimeSeconds,
    double MaxSpeed,
    double MaxAcceleration,
    double MaxDisplacement,
    double MaxDamage,
    int NonFiniteCount,
    bool StableEstimate
);
