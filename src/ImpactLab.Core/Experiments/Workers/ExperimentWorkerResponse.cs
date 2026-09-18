namespace ImpactLab.Core.Experiments.Workers;

public sealed record ExperimentWorkerResponse(
    int ProtocolVersion,
    string CaseId,
    bool Success,
    string? Error,
    string? ResultPath,
    TimeSpan ComputeTime,
    int ExitCode,
    WorkerFailureCode FailureCode = WorkerFailureCode.None,
    IReadOnlyDictionary<string, double>? Metrics = null,
    IReadOnlyList<WorkerArtifactReference>? Artifacts = null,
    string? WorkerId = null
);
