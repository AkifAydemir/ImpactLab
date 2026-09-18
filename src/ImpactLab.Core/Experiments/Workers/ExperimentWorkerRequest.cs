namespace ImpactLab.Core.Experiments.Workers;

public sealed record ExperimentWorkerRequest(
    int ProtocolVersion,
    string CaseId,
    string ScenarioPath,
    string BackendId,
    string OutputDirectory,
    WorkerResourceQuota Quota,
    IReadOnlyDictionary<string, double> Parameters,
    string RequestId = "",
    string? LeaseId = null
)
{
    public string EffectiveRequestId => string.IsNullOrWhiteSpace(RequestId) ? CaseId : RequestId;

    public void Validate()
    {
        ExperimentWorkerProtocol.ValidateVersion(ProtocolVersion);
        if (string.IsNullOrWhiteSpace(CaseId))
            throw new WorkerProtocolException("CaseId is required.");
        if (string.IsNullOrWhiteSpace(ScenarioPath))
            throw new WorkerProtocolException("ScenarioPath is required.");
        if (string.IsNullOrWhiteSpace(BackendId))
            throw new WorkerProtocolException("BackendId is required.");
        if (string.IsNullOrWhiteSpace(OutputDirectory))
            throw new WorkerProtocolException("OutputDirectory is required.");
        Quota.Validate();
    }
}
