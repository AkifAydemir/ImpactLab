namespace ImpactLab.Core.Thermal.Coupled;

public sealed record MonolithicStepResult(
    CoupledState State,
    bool Converged,
    int Iterations,
    IReadOnlyList<MonolithicNewtonIteration> History,
    double MechanicalEnergyJ,
    double ThermalEnergyJ,
    double PlasticHeatJ
);
