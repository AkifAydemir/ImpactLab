namespace ImpactLab.Core.PostProcessing;

public readonly record struct EnergyBalanceSample(
    double TimeSeconds,
    double MechanicalEnergyJ,
    double FrictionEnergyJ,
    double DiagnosticResidualJ,
    double ResidualPercent
);
