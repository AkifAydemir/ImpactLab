using System.Collections.Concurrent;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Experiments;
using ImpactLab.Core.Experiments.Workers;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;

namespace ImpactLab.App.Services;

public sealed class DistributedExperimentRunnerService
{
    private readonly ProcessExperimentWorkerService _workers;

    public DistributedExperimentRunnerService(ProcessExperimentWorkerService workers) =>
        _workers = workers;

    public async Task<ExperimentResult> RunAsync(
        ExperimentDefinition definition,
        MaterialCatalog materials,
        string scratchRoot,
        ExperimentExecutionPolicy policy,
        WorkerResourceQuota? quota = null,
        IProgress<ExperimentProgress>? progress = null,
        CancellationToken ct = default,
        string backendId = AdvancedLatticeBackend.BackendId
    )
    {
        policy.Validate();
        quota ??= WorkerResourceQuota.Default;
        quota.Validate();
        Directory.CreateDirectory(scratchRoot);
        var started = DateTimeOffset.UtcNow;
        var points = ParameterSweepPlanner.Expand(definition.Sweep);
        var results = new ConcurrentDictionary<int, ExperimentRunResult>();
        var failed = 0;
        var tasks = points
            .Select(async point =>
            {
                ct.ThrowIfCancellationRequested();
                var caseDir = Path.Combine(scratchRoot, $"case-{point.Index:D6}");
                Directory.CreateDirectory(caseDir);
                var scenarioPath = Path.Combine(caseDir, "scenario.json");
                ScenarioEnvelopeSerializer.Save(scenarioPath, definition.BaseScenario);
                var request = new ExperimentWorkerRequest(
                    ExperimentWorkerProtocol.Version,
                    $"case-{point.Index:D6}",
                    scenarioPath,
                    backendId,
                    caseDir,
                    quota,
                    point.Values,
                    $"{definition.Name}:{point.Index}"
                );
                var response = await _workers.RunAsync(request, ct).ConfigureAwait(false);
                var set = new ResultMetricSet();
                if (response.Metrics is not null)
                    foreach (var pair in response.Metrics)
                        if (Enum.TryParse<ResultMetricKind>(pair.Key, true, out var kind))
                            set.Set(kind, pair.Value);
                if (!response.Success)
                    Interlocked.Increment(ref failed);
                results[point.Index] = new ExperimentRunResult(
                    point,
                    set,
                    response.ComputeTime,
                    response.Error
                );
                progress?.Report(
                    new(
                        results.Count,
                        points.Count,
                        _workers.Statistics.Running,
                        Volatile.Read(ref failed),
                        point.Label
                    )
                );
            })
            .ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        return new ExperimentResult(
            definition.Name,
            started,
            DateTimeOffset.UtcNow,
            results.Values.OrderBy(x => x.Point.Index).ToArray()
        );
    }
}
