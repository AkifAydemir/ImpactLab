namespace ImpactLab.Core.Continuum.Contact;

public sealed record AugmentedContactSettings(
    double InitialPenaltyNPerM = 1e8,
    double AugmentationFactor = 1.0,
    double MaximumPenaltyNPerM = 1e12,
    double GapToleranceMeters = 1e-7,
    double MultiplierToleranceN = 1e-4,
    int MaxAugmentations = 8,
    double FrictionCoefficient = 0.25,
    double StickToleranceMps = 1e-4
)
{
    public void Validate()
    {
        if (
            InitialPenaltyNPerM <= 0
            || MaximumPenaltyNPerM < InitialPenaltyNPerM
            || GapToleranceMeters <= 0
            || MaxAugmentations < 1
        )
            throw new InvalidOperationException("Invalid augmented contact settings.");
    }
}
