namespace ImpactLab.Core.Diagnostics;

public sealed record StabilityEstimate(
    double RequestedTimeStepSeconds,
    double EstimatedCriticalTimeStepSeconds,
    double SafetyRatio,
    bool IsLikelyStable,
    string Message
);
