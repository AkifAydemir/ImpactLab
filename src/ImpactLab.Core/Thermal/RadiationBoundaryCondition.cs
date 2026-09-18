namespace ImpactLab.Core.Thermal;

public sealed record RadiationBoundaryCondition(
    string Id,
    string? PartId,
    double AmbientTemperatureKelvin,
    double Emissivity
)
{
    public void Validate()
    {
        if (AmbientTemperatureKelvin <= 0 || Emissivity is < 0 or > 1)
            throw new InvalidOperationException("Invalid radiation boundary.");
    }
}
