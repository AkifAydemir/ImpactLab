namespace ImpactLab.Core.Results.Events;

public sealed record ResultEvent(
    string Id,
    string Name,
    double TimeSeconds,
    int FrameIndex,
    ResultEventSeverity Severity,
    string Message,
    IReadOnlyDictionary<string, double> Values
);
