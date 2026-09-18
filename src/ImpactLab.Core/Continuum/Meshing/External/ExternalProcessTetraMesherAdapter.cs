using System.Diagnostics;

namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed class ExternalProcessTetraMesherAdapter : IExternalTetraMesherAdapter
{
    private readonly ExternalProcessMesherSettings _settings;
    private readonly ExternalMesherValidationPolicy _validation;

    public ExternalProcessTetraMesherAdapter(
        string id,
        ExternalProcessMesherSettings settings,
        ExternalMesherValidationPolicy? validation = null
    )
    {
        Id = string.IsNullOrWhiteSpace(id)
            ? throw new ArgumentException("Adapter id is required.", nameof(id))
            : id;
        _settings = settings;
        _validation = validation ?? new();
        _settings.Validate();
    }

    public string Id { get; }

    public ExternalTetraMesherResult Generate(
        ExternalTetraMesherRequest request,
        CancellationToken ct = default
    )
    {
        request.Validate();
        Directory.CreateDirectory(request.WorkingDirectory);
        var requestPath = Path.Combine(request.WorkingDirectory, "mesher-request.json");
        var resultPath = Path.Combine(request.WorkingDirectory, "mesher-result.json");
        ExternalMesherArtifactProtocol.WriteRequest(requestPath, request);
        var sw = Stopwatch.StartNew();
        var stdout = string.Empty;
        var stderr = string.Empty;
        var exitCode = -1;
        try
        {
            var psi = new ProcessStartInfo(_settings.ExecutablePath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = request.WorkingDirectory,
                Arguments = Expand(_settings.ArgumentsTemplate, requestPath, resultPath),
            };
            using var process =
                Process.Start(psi)
                ?? throw new InvalidOperationException("Could not start external tetra mesher.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(_settings.EffectiveTimeout);
            var outTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var errTask = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                process.WaitForExitAsync(timeout.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }
            stdout = outTask.GetAwaiter().GetResult();
            stderr = errTask.GetAwaiter().GetResult();
            exitCode = process.ExitCode;
            sw.Stop();
            var artifacts = Manifest(requestPath, File.Exists(resultPath) ? resultPath : null);
            if (exitCode != 0)
                return new(
                    false,
                    null,
                    stdout,
                    stderr,
                    sw.Elapsed,
                    exitCode,
                    artifacts,
                    null,
                    $"External mesher exited with code {exitCode}."
                );
            if (!File.Exists(resultPath))
                return new(
                    false,
                    null,
                    stdout,
                    stderr,
                    sw.Elapsed,
                    exitCode,
                    artifacts,
                    null,
                    "External mesher did not create its result artifact."
                );
            var artifact = ExternalMesherArtifactProtocol.ReadResult(resultPath);
            if (!artifact.Success)
                return new(
                    false,
                    null,
                    stdout,
                    stderr,
                    sw.Elapsed,
                    exitCode,
                    artifacts,
                    null,
                    artifact.Error ?? "External mesher reported failure."
                );
            var mesh = ExternalMesherArtifactProtocol.ToMesh(artifact, request.Material);
            var validation = ExternalMesherResultValidator.Validate(mesh, _validation);
            var success = validation.Passed;
            if (success && !_settings.KeepArtifactsOnSuccess)
                TryDelete(requestPath, resultPath);
            return new(
                success,
                mesh,
                stdout,
                stderr,
                sw.Elapsed,
                exitCode,
                artifacts,
                validation,
                success ? null : string.Join("; ", validation.Findings)
            );
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            return new(
                false,
                null,
                stdout,
                stderr,
                sw.Elapsed,
                exitCode,
                Manifest(requestPath, File.Exists(resultPath) ? resultPath : null),
                null,
                $"External mesher exceeded timeout {_settings.EffectiveTimeout}."
            );
        }
    }

    private static string Expand(string template, string input, string output) =>
        template
            .Replace("{input}", Quote(input), StringComparison.Ordinal)
            .Replace("{output}", Quote(output), StringComparison.Ordinal);

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(true);
        }
        catch { }
    }

    private static void TryDelete(params string[] paths)
    {
        foreach (var path in paths)
            try
            {
                File.Delete(path);
            }
            catch { }
    }

    private static ExternalMesherArtifactManifest Manifest(
        string requestPath,
        string? resultPath
    ) =>
        new(
            requestPath,
            ExternalMesherArtifactProtocol.Sha256(requestPath),
            resultPath,
            resultPath is null ? null : ExternalMesherArtifactProtocol.Sha256(resultPath)
        );
}
