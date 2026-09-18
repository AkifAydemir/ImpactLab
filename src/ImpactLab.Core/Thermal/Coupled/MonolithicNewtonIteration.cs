namespace ImpactLab.Core.Thermal.Coupled;

public readonly record struct MonolithicNewtonIteration(
    int Iteration,
    double MechanicalResidual,
    double ThermalResidual,
    double CombinedResidual,
    double IncrementNorm,
    double Scale,
    bool Converged
);
