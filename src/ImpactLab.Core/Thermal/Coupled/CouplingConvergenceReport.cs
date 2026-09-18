namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CouplingConvergenceReport(
    int Steps,
    int FailedSteps,
    int TotalNewtonIterations,
    double MaxEnergyImbalanceJ
);
