namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CoupledEnergyLedger(
    double MechanicalInternalJ,
    double KineticJ,
    double ThermalInternalJ,
    double PlasticDissipationJ,
    double ContactDissipationJ,
    double BoundaryHeatJ,
    double ImbalanceJ
);
