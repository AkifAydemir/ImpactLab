namespace ImpactLab.Core.Experiments.Workers;

public enum WorkerFailureCode
{
    None,
    InvalidRequest,
    ScenarioLoad,
    BackendExecution,
    ArtifactWrite,
    QuotaExceeded,
    Cancelled,
    Protocol,
    Unknown,
}
