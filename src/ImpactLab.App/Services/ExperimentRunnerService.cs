using ImpactLab.Core.Analysis;
using ImpactLab.Core.Experiments;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed class ExperimentRunnerService
{
    private readonly SimulationRunnerService _simulation;

    public ExperimentRunnerService(SimulationRunnerService simulation) => _simulation = simulation;

    public async Task<ExperimentResult> RunAsync(
        ExperimentDefinition definition,
        MaterialCatalog materials,
        IProgress<(int Done, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var started = DateTimeOffset.Now;
        var points = ParameterSweepPlanner.Expand(definition.Sweep);
        var runs = new List<ExperimentRunResult>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var point = points[i];
            progress?.Report((i, points.Count, point.Label));
            try
            {
                var scenario = ScenarioCloner.Clone(definition.BaseScenario);
                foreach (var pair in point.Values)
                    ScenarioValuePath.Apply(scenario, pair.Key, pair.Value);
                var bundle = await Task.Run(
                    () => _simulation.Run(scenario, materials, cancellationToken),
                    cancellationToken
                );
                runs.Add(
                    new ExperimentRunResult(
                        point,
                        ResultMetricExtractor.Extract(bundle.Summary),
                        bundle.Result.ComputeTime
                    )
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                runs.Add(
                    new ExperimentRunResult(point, new ResultMetricSet(), TimeSpan.Zero, ex.Message)
                );
            }
        }
        progress?.Report((points.Count, points.Count, "Complete"));
        return new ExperimentResult(definition.Name, started, DateTimeOffset.Now, runs);
    }
}
