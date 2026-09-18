namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CoupledState(
    double[] Displacement,
    double[] TemperatureKelvin,
    double TimeSeconds,
    double[] Velocity,
    double[] Acceleration
);
