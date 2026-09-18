using System.Diagnostics;

namespace ImpactLab.Core.Experiments.Workers;

public sealed class ProcessExperimentWorker
{
    public ProcessExperimentWorker(string executable)
    {
        if (string.IsNullOrWhiteSpace(executable))
            throw new ArgumentException("Worker executable is required.", nameof(executable));
        Executable = executable;
    }

    public string Executable { get; }
    public event EventHandler<WorkerHeartbeat>? Heartbeat;

    public async Task<ExperimentWorkerResponse> RunAsync(
        ExperimentWorkerRequest request,
        CancellationToken ct = default
    )
    {
        request.Validate();
        var psi = new ProcessStartInfo(Executable)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("worker");
        using var process =
            Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start experiment worker.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(request.Quota.WallTime);
        var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process
                .StandardInput.WriteLineAsync(WorkerJsonProtocol.Serialize(request))
                .ConfigureAwait(false);
            process.StandardInput.Close();
            while (true)
            {
                var line = await process
                    .StandardOutput.ReadLineAsync(timeout.Token)
                    .ConfigureAwait(false);
                if (line is null)
                    break;
                WorkerProtocolEnvelope envelope;
                try
                {
                    envelope = WorkerJsonProtocol.Deserialize<WorkerProtocolEnvelope>(line);
                }
                catch (WorkerProtocolException)
                {
                    // v1 compatibility: accept a direct terminal response.
                    var direct = WorkerJsonProtocol.Deserialize<ExperimentWorkerResponse>(line);
                    await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                    return direct with { ExitCode = process.ExitCode };
                }
                ExperimentWorkerProtocol.ValidateVersion(envelope.ProtocolVersion);
                if (
                    envelope.CorrelationId != request.EffectiveRequestId
                    && envelope.CorrelationId != request.CaseId
                )
                    continue;
                if (envelope.Kind == WorkerMessageKind.Heartbeat)
                {
                    Heartbeat?.Invoke(this, WorkerJsonProtocol.Payload<WorkerHeartbeat>(envelope));
                    continue;
                }
                if (envelope.Kind == WorkerMessageKind.Hello)
                    continue;
                if (envelope.Kind == WorkerMessageKind.Response)
                {
                    var response = WorkerJsonProtocol.Payload<ExperimentWorkerResponse>(envelope);
                    await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                    return response with { ExitCode = process.ExitCode };
                }
            }
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);
            return new(
                request.ProtocolVersion,
                request.CaseId,
                false,
                string.IsNullOrWhiteSpace(stderr)
                    ? "Worker produced no terminal response."
                    : stderr,
                null,
                TimeSpan.Zero,
                process.ExitCode,
                WorkerFailureCode.Protocol
            );
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch { }
            return new(
                request.ProtocolVersion,
                request.CaseId,
                false,
                ct.IsCancellationRequested
                    ? "Worker cancelled."
                    : "Worker wall-time quota exceeded.",
                null,
                TimeSpan.Zero,
                -1,
                ct.IsCancellationRequested
                    ? WorkerFailureCode.Cancelled
                    : WorkerFailureCode.QuotaExceeded
            );
        }
    }
}
