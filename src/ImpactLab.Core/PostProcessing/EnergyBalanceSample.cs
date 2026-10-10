namespace ImpactLab.Core.PostProcessing;

public readonly record struct EnergyBalanceSample(
    double TimeSeconds,
    double MechanicalEnergyJ,
    double FrictionEnergyJ,
    double DiagnosticResidualJ,
    double ResidualPercent,
    double ContactPotentialEnergyJ = 0.0,
    double ContactDampingEnergyJ = 0.0,
    double PlasticDissipationEnergyJ = 0.0,
    double FractureDissipationEnergyJ = 0.0,
    double ThermalEnergyJ = 0.0,
    double ExternalWorkJ = 0.0,
    double ConstraintWorkJ = 0.0,
    double AccountedEnergyJ = 0.0,
    double StructuralDampingEnergyJ = 0.0,
    double GlobalDampingEnergyJ = 0.0
);
