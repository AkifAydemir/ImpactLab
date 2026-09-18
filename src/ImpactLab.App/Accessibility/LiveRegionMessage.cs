namespace ImpactLab.App.Accessibility;

public sealed record LiveRegionMessage(
    string Id,
    string Text,
    DateTimeOffset Timestamp,
    bool Assertive = false
);
