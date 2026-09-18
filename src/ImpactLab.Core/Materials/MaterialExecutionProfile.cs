namespace ImpactLab.Core.Materials;

public sealed record MaterialExecutionProfile(
    string MaterialId,
    string ConstitutiveLawId,
    double InitialTemperatureKelvin = 293.15,
    double TemperatureRisePerPlasticEnergy = 0.0
)
{
    public void Validate()
    {
        if (
            string.IsNullOrWhiteSpace(MaterialId)
            || string.IsNullOrWhiteSpace(ConstitutiveLawId)
            || InitialTemperatureKelvin <= 0
        )
            throw new InvalidOperationException("Invalid material execution profile.");
    }
}
