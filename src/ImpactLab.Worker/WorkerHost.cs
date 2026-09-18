using ImpactLab.Core.Backends;
using ImpactLab.Core.Experiments.Workers;

namespace ImpactLab.Worker;

public sealed class WorkerHost : IDisposable
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly string _workerId = $"{Environment.MachineName}-{Environment.ProcessId}";

    public WorkerHost(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        var line = await _input.ReadLineAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(line))
            return WorkerExitCodes.InvalidRequest;
        ExperimentWorkerRequest request;
        try
        {
            request = WorkerJsonProtocol.Deserialize<ExperimentWorkerRequest>(line);
            request.Validate();
        }
        catch (Exception ex)
        {
            await WriteResponse(
                "invalid",
                new ExperimentWorkerResponse(
                    ExperimentWorkerProtocol.Version,
                    "invalid",
                    false,
                    ex.Message,
                    null,
                    TimeSpan.Zero,
                    WorkerExitCodes.InvalidRequest,
                    WorkerFailureCode.InvalidRequest
                ),
                ct
            );
            return WorkerExitCodes.InvalidRequest;
        }
        var registry = SimulationBackendRegistry.CreateDefault();
        var capabilities = new WorkerCapabilities(
            "v14",
            Environment.Version.ToString(),
            Environment.OSVersion.ToString(),
            Environment.ProcessorCount,
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            registry.Describe().Select(x => x.Id).ToArray()
        );
        await WriteEnvelope(
            WorkerMessageKind.Hello,
            request.EffectiveRequestId,
            new WorkerHello(_workerId, capabilities, Environment.ProcessId),
            ct
        );
        await using var heartbeat = new WorkerHeartbeatPump(
            _output,
            _workerId,
            request.CaseId,
            request.EffectiveRequestId,
            _writeGate
        );
        heartbeat.Start();
        var response = await new WorkerCaseExecutor()
            .ExecuteAsync(request, ct)
            .ConfigureAwait(false);
        await WriteResponse(request.EffectiveRequestId, response with { WorkerId = _workerId }, ct);
        return response.Success ? WorkerExitCodes.Success : response.ExitCode;
    }

    private Task WriteResponse(
        string correlation,
        ExperimentWorkerResponse response,
        CancellationToken ct
    ) => WriteEnvelope(WorkerMessageKind.Response, correlation, response, ct);

    private async Task WriteEnvelope<T>(
        WorkerMessageKind kind,
        string correlation,
        T value,
        CancellationToken ct
    )
    {
        await _writeGate.WaitAsync(ct);
        try
        {
            await _output.WriteLineAsync(WorkerJsonProtocol.Envelope(kind, correlation, value));
            await _output.FlushAsync(ct);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public void Dispose() => _writeGate.Dispose();
}
