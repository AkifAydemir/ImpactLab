namespace ImpactLab.Core.Thermal.Coupled;

public sealed record MonolithicAssemblyResult(
    CoupledBlockSystem Blocks,
    CoupledResidual Residual,
    double MechanicalEnergyJ,
    double ThermalEnergyJ,
    double PlasticHeatJ
);
