namespace ImpactLab.Core.Geometry.Exchange;

public sealed record CadTessellationSettings(
    double ChordToleranceMeters = 1e-4,
    double AngularToleranceDegrees = 10,
    double MaxEdgeLengthMeters = 0.02,
    bool Heal = true
);
