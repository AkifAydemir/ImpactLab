namespace ImpactLab.Core.Materials;

public sealed record CohesiveInterfaceLaw(
    string Id,
    double NormalStiffnessPaPerM = 1e14,
    double ShearStiffnessPaPerM = 5e13,
    double DamageOnsetSeparationMeters = 2e-5,
    double FailureSeparationMeters = 2e-4,
    double MixedModeExponent = 2.0,
    double ResidualTractionFraction = 0.0
)
{
    public void Validate()
    {
        if (
            string.IsNullOrWhiteSpace(Id)
            || NormalStiffnessPaPerM <= 0
            || ShearStiffnessPaPerM <= 0
        )
            throw new InvalidOperationException("Invalid cohesive law stiffness.");
        if (
            DamageOnsetSeparationMeters <= 0
            || FailureSeparationMeters <= DamageOnsetSeparationMeters
        )
            throw new InvalidOperationException("Invalid cohesive separation range.");
        if (MixedModeExponent <= 0 || ResidualTractionFraction is < 0 or > 1)
            throw new InvalidOperationException("Invalid cohesive law parameters.");
    }
}
