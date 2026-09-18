namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CoupledResidual(
    double[] Mechanical,
    double[] Thermal,
    double MechanicalNorm,
    double ThermalNorm,
    double CombinedNorm
);
