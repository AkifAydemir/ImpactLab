namespace ImpactLab.Core.Diagnostics;

public readonly record struct PerformanceSample(
    PerformanceStage Stage,
    string Name,
    TimeSpan Elapsed,
    long WorkUnits,
    long ManagedBytesDelta,
    DateTimeOffset StartedUtc
);
