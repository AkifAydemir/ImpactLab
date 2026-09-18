using System.Collections.Concurrent;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Experiments;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed class ParallelExperimentRunnerService
{
    private readonly SimulationRunnerService _simulation;

    public ParallelExperimentRunnerService(SimulationRunnerService simulation) =>
        _simulation = simulation;

    public async Task<ExperimentResult> RunAsync(
        ExperimentDefinition definition,
        MaterialCatalog materials,
        ExperimentExecutionPolicy policy,
        ExperimentCheckpointStore? checkpointStore = null,
        IProgress<ExperimentProgress>? progress = null,
        CancellationToken cancellationToken = default,
        string backendId = ImpactLab.Core.Backends.AdvancedLatticeBackend.BackendId
    )
    {
        policy.Validate();
        var started = DateTimeOffset.UtcNow;
        var points = ParameterSweepPlanner.Expand(definition.Sweep);
        var checkpoint = policy.ResumeFromCheckpoint ? checkpointStore?.Load() : null;
        var completed = new ConcurrentDictionary<int, ExperimentRunResult>();
        if (checkpoint is not null)
            foreach (var r in checkpoint.CompletedRuns)
                completed[r.Point.Index] = r;
        var queue = points.Where(x => !completed.ContainsKey(x.Index)).ToArray();
        var failed = completed.Values.Count(x => !x.Succeeded);
        var running = 0;
        var sinceCheckpoint = 0;
        using var gate = new SemaphoreSlim(policy.MaxDegreeOfParallelism);
        var tasks = queue
            .Select(async point =>
            {
                await gate.WaitAsync(cancellationToken);
                Interlocked.Increment(ref running);
                progress?.Report(
                    new(
                        completed.Count,
                        points.Count,
                        running,
                        Volatile.Read(ref failed),
                        point.Label
                    )
                );
                try
                {
                    var scenario = ScenarioCloner.Clone(definition.BaseScenario);
                    ScenarioParameterBinder.Apply(scenario, ScenarioParameterBinder.From(point));
                    var bundle = await Task.Run(
                        () => _simulation.Run(scenario, materials, cancellationToken, backendId),
                        cancellationToken
                    );
                    completed[point.Index] = new ExperimentRunResult(
                        point,
                        ResultMetricExtractor.Extract(bundle.Summary),
                        bundle.Result.ComputeTime
                    );
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Interlocked.Increment(ref failed);
                    completed[point.Index] = new ExperimentRunResult(
                        point,
                        new ResultMetricSet(),
                        TimeSpan.Zero,
                        ex.Message
                    );
                    if (!policy.ContinueOnFailure)
                        throw;
                }
                finally
                {
                    Interlocked.Decrement(ref running);
                    var done = completed.Count;
                    progress?.Report(
                        new(
                            done,
                            points.Count,
                            running,
                            Volatile.Read(ref failed),
                            $"{done}/{points.Count}"
                        )
                    );
                    if (
                        checkpointStore is not null
                        && Interlocked.Increment(ref sinceCheckpoint) >= policy.CheckpointEveryRuns
                    )
                    {
                        Interlocked.Exchange(ref sinceCheckpoint, 0);
                        checkpointStore.Save(
                            new ExperimentCheckpoint(
                                definition.Name,
                                DateTimeOffset.UtcNow,
                                completed.Values.OrderBy(x => x.Point.Index).ToArray(),
                                completed.Keys.Order().ToArray()
                            )
                        );
                    }
                    gate.Release();
                }
            })
            .ToArray();
        await Task.WhenAll(tasks);
        var result = new ExperimentResult(
            definition.Name,
            started,
            DateTimeOffset.UtcNow,
            completed.Values.OrderBy(x => x.Point.Index).ToArray()
        );
        checkpointStore?.Delete();
        return result;
    }
}
