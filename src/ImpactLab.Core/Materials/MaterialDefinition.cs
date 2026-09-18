namespace ImpactLab.Core.Materials;

public sealed record MaterialDefinition(
    string Id,
    string DisplayName,
    double DensityKgPerM3,
    double YoungModulusPa,
    double PoissonRatio,
    double YieldStrain,
    double FailureStrain,
    double DampingRatio,
    MaterialResponseKind ResponseKind = MaterialResponseKind.Generic,
    double TangentModulusPa = 0.0,
    double CompressiveStrengthScale = 1.0,
    double RateSensitivity = 0.0
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Material Id cannot be empty.");
        if (DensityKgPerM3 <= 0.0)
            throw new InvalidOperationException("Density must be positive.");
        if (YoungModulusPa <= 0.0)
            throw new InvalidOperationException("Young modulus must be positive.");
        if (PoissonRatio is <= -1.0 or >= 0.5)
            throw new InvalidOperationException("Poisson ratio must be between -1 and 0.5.");
        if (YieldStrain <= 0.0)
            throw new InvalidOperationException("Yield strain must be positive.");
        if (FailureStrain <= YieldStrain)
            throw new InvalidOperationException("Failure strain must exceed yield strain.");
        if (DampingRatio < 0.0)
            throw new InvalidOperationException("Damping ratio cannot be negative.");
        if (TangentModulusPa < 0.0)
            throw new InvalidOperationException("Tangent modulus cannot be negative.");
        if (CompressiveStrengthScale <= 0.0)
            throw new InvalidOperationException("Compressive strength scale must be positive.");
        if (RateSensitivity < 0.0)
            throw new InvalidOperationException("Rate sensitivity cannot be negative.");
    }
}
