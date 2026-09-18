using ImpactLab.Core.Analysis;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Experiments;

public sealed class ExperimentDefinition
{
    public string Name { get; set; } = "Experiment";
    public ScenarioDefinition BaseScenario { get; set; } = new();
    public SweepDefinition Sweep { get; set; } = new();
    public ResultMetricKind RankingMetric { get; set; } = ResultMetricKind.MaxDisplacementMeters;
    public bool LowerIsBetter { get; set; } = true;
}
