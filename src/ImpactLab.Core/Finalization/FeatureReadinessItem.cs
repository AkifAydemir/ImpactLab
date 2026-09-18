namespace ImpactLab.Core.Finalization;

public sealed record FeatureReadinessItem(
    FeatureArea Area,
    FeatureAreaStatus Status,
    string Evidence,
    string? BlockingIssue,
    bool RequiredForCompletion = true
);
