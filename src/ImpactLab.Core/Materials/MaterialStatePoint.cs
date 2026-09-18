namespace ImpactLab.Core.Materials;

public readonly record struct MaterialStatePoint(
    double Strain,
    double StrainRatePerSecond,
    double TemperatureKelvin,
    double Damage = 0.0
)
{
    public void Validate()
    {
        if (
            !double.IsFinite(Strain)
            || !double.IsFinite(StrainRatePerSecond)
            || !double.IsFinite(TemperatureKelvin)
            || TemperatureKelvin <= 0
        )
            throw new InvalidOperationException("Invalid material state point.");
    }
}
