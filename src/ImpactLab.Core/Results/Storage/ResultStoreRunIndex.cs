namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreRunIndex(
    string RunId,
    string ScenarioFingerprint,
    IReadOnlyList<string> Fields,
    int FrameCount,
    double StartTime,
    double EndTime,
    long TotalBytes,
    DateTimeOffset CreatedUtc
);
