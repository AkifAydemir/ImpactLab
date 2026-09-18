namespace ImpactLab.Core.Experiments.Workers;

public sealed class ExperimentWorkerPool : IDisposable
{
    private readonly SemaphoreSlim _gate;
    private readonly ProcessExperimentWorker _worker;
    private long _submitted,
        _completed,
        _failed;
    private int _running;

    public ExperimentWorkerPool(ProcessExperimentWorker worker, int concurrency)
    {
        _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        Capacity = Math.Max(1, concurrency);
        _gate = new(Capacity);
    }

    public int Capacity { get; }
    public WorkerPoolStatistics Statistics =>
        new(
            Interlocked.Read(ref _submitted),
            Interlocked.Read(ref _completed),
            Interlocked.Read(ref _failed),
            Volatile.Read(ref _running),
            Capacity
        );

    public async Task<ExperimentWorkerResponse> RunAsync(
        ExperimentWorkerRequest request,
        CancellationToken ct = default
    )
    {
        Interlocked.Increment(ref _submitted);
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        Interlocked.Increment(ref _running);
        try
        {
            var result = await _worker.RunAsync(request, ct).ConfigureAwait(false);
            Interlocked.Increment(ref _completed);
            if (!result.Success)
                Interlocked.Increment(ref _failed);
            return result;
        }
        finally
        {
            Interlocked.Decrement(ref _running);
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
