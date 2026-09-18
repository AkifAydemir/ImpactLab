using System.Diagnostics;
using ImpactLab.Core.Experiments.Workers;

namespace ImpactLab.Worker;

public sealed class WorkerHeartbeatPump : IAsyncDisposable
{
    private readonly TextWriter _writer;
    private readonly string _workerId;
    private readonly string _caseId;
    private readonly string _correlationId;
    private readonly SemaphoreSlim _writeGate;
    private readonly CancellationTokenSource _stop = new();
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private Task? _loop;

    public WorkerHeartbeatPump(
        TextWriter writer,
        string workerId,
        string caseId,
        string correlationId,
        SemaphoreSlim writeGate
    )
    {
        _writer = writer;
        _workerId = workerId;
        _caseId = caseId;
        _correlationId = correlationId;
        _writeGate = writeGate;
    }

    public void Start() => _loop = Loop();

    private async Task Loop()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), _stop.Token);
                var hb = new WorkerHeartbeat(
                    _workerId,
                    _caseId,
                    _sw.Elapsed,
                    Environment.WorkingSet,
                    Environment.ProcessId
                );
                await _writeGate.WaitAsync(_stop.Token);
                try
                {
                    await _writer.WriteLineAsync(
                        WorkerJsonProtocol.Envelope(WorkerMessageKind.Heartbeat, _correlationId, hb)
                    );
                    await _writer.FlushAsync();
                }
                finally
                {
                    _writeGate.Release();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        if (_loop is not null)
            try
            {
                await _loop;
            }
            catch (OperationCanceledException) { }
        _stop.Dispose();
    }
}
