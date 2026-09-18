namespace ImpactLab.Core.Continuum.Contact;

public sealed class ContactAugmentationController
{
    private readonly AugmentedContactSettings _s;

    public ContactAugmentationController(AugmentedContactSettings s) => _s = s;

    public double NextPenalty(double current, double maxGap) =>
        maxGap <= _s.GapToleranceMeters
            ? current
            : Math.Min(_s.MaximumPenaltyNPerM, current * Math.Max(1.2, 1 + _s.AugmentationFactor));

    public bool Converged(double maxGap, double multiplierChange) =>
        maxGap <= _s.GapToleranceMeters && multiplierChange <= _s.MultiplierToleranceN;
}
