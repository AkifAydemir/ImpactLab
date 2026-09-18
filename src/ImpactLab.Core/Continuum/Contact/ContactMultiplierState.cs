namespace ImpactLab.Core.Continuum.Contact;

public sealed record ContactMultiplierState(
    double NormalMultiplierN,
    double TangentialX,
    double TangentialY,
    bool Sticking,
    double LastGapMeters,
    int Age
);
