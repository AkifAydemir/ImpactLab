namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactIterationReport(
    int NewtonIteration,
    int Augmentation,
    IReadOnlyList<ContactPatchStatistics> Patches,
    double MaxGap,
    double MultiplierChange,
    bool Converged
);
