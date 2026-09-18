namespace ImpactLab.Core.Thermal;

public sealed record ConvectionBoundaryCondition(
    string Id,
    string? PartId,
    double AmbientTemperatureKelvin,
    double FilmCoefficientWPerM2K
)
{
    public void Validate()
    {
        if (AmbientTemperatureKelvin <= 0 || FilmCoefficientWPerM2K < 0)
            throw new InvalidOperationException("Invalid convection boundary.");
    }
}
