namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactPatchStatistics(
    int Points,
    double AreaM2,
    double MeanPressurePa,
    double PeakPressurePa,
    double StickFraction
);
