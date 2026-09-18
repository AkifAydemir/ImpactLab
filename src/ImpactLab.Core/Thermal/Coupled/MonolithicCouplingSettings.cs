namespace ImpactLab.Core.Thermal.Coupled;

public sealed record MonolithicCouplingSettings(
    double ResidualTolerance = 1e-6,
    double IncrementTolerance = 1e-7,
    int MaxNewtonIterations = 25,
    double MechanicalScale = 1.0,
    double ThermalScale = 1e-3,
    double LineSearchMinimum = 0.05,
    bool IncludePlasticHeat = true,
    bool IncludeThermalExpansion = true
)
{
    public void Validate()
    {
        if (ResidualTolerance <= 0 || IncrementTolerance <= 0 || MaxNewtonIterations < 1)
            throw new InvalidOperationException("Invalid monolithic coupling settings.");
    }
}
