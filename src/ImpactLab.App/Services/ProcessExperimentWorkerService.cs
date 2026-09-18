using ImpactLab.Core.Experiments.Workers;

namespace ImpactLab.App.Services;

public sealed class ProcessExperimentWorkerService : IDisposable
{
    private readonly ExperimentWorkerPool _pool;

    public ProcessExperimentWorkerService(string executable, int concurrency)
    {
        var worker = new ProcessExperimentWorker(executable);
        worker.Heartbeat += (s, e) => Heartbeat?.Invoke(this, e);
        _pool = new(worker, concurrency);
    }

    public event EventHandler<WorkerHeartbeat>? Heartbeat;
    public WorkerPoolStatistics Statistics => _pool.Statistics;

    public Task<ExperimentWorkerResponse> RunAsync(
        ExperimentWorkerRequest request,
        CancellationToken ct = default
    ) => _pool.RunAsync(request, ct);

    public void Dispose() => _pool.Dispose();
}
