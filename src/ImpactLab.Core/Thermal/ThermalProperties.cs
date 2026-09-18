namespace ImpactLab.Core.Thermal;

public sealed record ThermalProperties(
    double ConductivityWPerMK,
    double SpecificHeatJPerKgK,
    double ThermalExpansionPerKelvin
)
{
    public static ThermalProperties SteelDefault { get; } = new(45, 480, 12e-6);

    public void Validate()
    {
        if (ConductivityWPerMK <= 0 || SpecificHeatJPerKgK <= 0)
            throw new InvalidOperationException(
                "Thermal conductivity and heat capacity must be positive."
            );
    }
}
