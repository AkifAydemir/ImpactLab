namespace ImpactLab.Core.Experiments.Workers;

public static class ExperimentWorkerProtocol
{
    public const int Version = 2;
    public const string RequestType = "impactlab.worker.request";
    public const string ResponseType = "impactlab.worker.response";
    public const string HelloType = "impactlab.worker.hello";
    public const string HeartbeatType = "impactlab.worker.heartbeat";

    public static void ValidateVersion(int version)
    {
        if (version != Version)
            throw new WorkerProtocolException(
                $"Worker protocol {version} is not supported; expected {Version}."
            );
    }
}
