using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Experiments;
using ImpactLab.Core.Experiments.Workers;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Worker;

public sealed class WorkerCaseExecutor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly SimulationBackendRegistry _backends =
        SimulationBackendRegistry.CreateDefault();

    public async Task<ExperimentWorkerResponse> ExecuteAsync(
        ExperimentWorkerRequest request,
        CancellationToken ct
    )
    {
        request.Validate();
        var sw = Stopwatch.StartNew();
        try
        {
            var scenario = ScenarioEnvelopeSerializer.Load(request.ScenarioPath);
            foreach (var pair in request.Parameters)
                ScenarioValuePath.Apply(scenario, pair.Key, pair.Value);
            var materials = new MaterialCatalog();
            var compiled = ScenarioCompiler.Compile(scenario, materials);
            var backend = _backends.Create(request.BackendId);
            var output = await Task.Run(
                    () =>
                        backend.Run(
                            new SimulationExecutionRequest(
                                compiled,
                                scenario.Settings,
                                scenario.ContactSettings,
                                scenario,
                                materials
                            ),
                            ct
                        ),
                    ct
                )
                .ConfigureAwait(false);
            var summary = BackendResultAnalyzer.Analyze(backend.Descriptor.Id, output);
            var metrics = ResultMetricExtractor
                .Extract(summary.Compatibility)
                .Values.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value,
                    StringComparer.OrdinalIgnoreCase
                );
            Directory.CreateDirectory(request.OutputDirectory);
            var artifactPath = Path.Combine(
                request.OutputDirectory,
                $"{Safe(request.CaseId)}.worker-result.json"
            );
            var artifact = new WorkerResultArtifact(
                request.CaseId,
                backend.Descriptor.Id,
                summary.DomainKind,
                metrics,
                sw.Elapsed,
                DateTimeOffset.UtcNow
            );
            await File.WriteAllTextAsync(
                    artifactPath,
                    JsonSerializer.Serialize(artifact, SerializerOptions),
                    ct
                )
                .ConfigureAwait(false);
            var info = new FileInfo(artifactPath);
            var hash = Convert
                .ToHexString(
                    SHA256.HashData(
                        await File.ReadAllBytesAsync(artifactPath, ct).ConfigureAwait(false)
                    )
                )
                .ToLowerInvariant();
            var reference = new WorkerArtifactReference(
                "worker-result",
                artifactPath,
                info.Length,
                hash
            );
            sw.Stop();
            return new(
                ExperimentWorkerProtocol.Version,
                request.CaseId,
                true,
                null,
                artifactPath,
                sw.Elapsed,
                WorkerExitCodes.Success,
                WorkerFailureCode.None,
                metrics,
                [reference],
                Environment.MachineName
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new(
                ExperimentWorkerProtocol.Version,
                request.CaseId,
                false,
                ex.Message,
                null,
                sw.Elapsed,
                WorkerExitCodes.BackendFailure,
                WorkerFailureCode.BackendExecution
            );
        }
    }

    private static string Safe(string value) =>
        string.Concat(value.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
}
