namespace ImpactLab.Worker;

public sealed record WorkerResultArtifact(
    string CaseId,
    string BackendId,
    string DomainKind,
    IReadOnlyDictionary<string, double> Metrics,
    TimeSpan ComputeTime,
    DateTimeOffset CompletedUtc
);
