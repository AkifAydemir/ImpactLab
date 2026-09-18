namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerHeartbeat(
    string WorkerId,
    string CaseId,
    TimeSpan Elapsed,
    long WorkingSetBytes,
    int ProcessId
);
