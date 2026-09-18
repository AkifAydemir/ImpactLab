namespace ImpactLab.Worker;

public static class WorkerExitCodes
{
    public const int Success = 0,
        InvalidRequest = 10,
        ScenarioLoad = 20,
        BackendFailure = 30,
        ArtifactFailure = 40,
        Unhandled = 99;
}
