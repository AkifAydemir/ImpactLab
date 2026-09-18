using ImpactLab.Core.Analysis;

namespace ImpactLab.Core.Experiments;

public sealed record ExperimentRunResult(
    SweepPoint Point,
    ResultMetricSet Metrics,
    TimeSpan ComputeTime,
    string? Error = null
)
{
    public bool Succeeded => string.IsNullOrWhiteSpace(Error);
}
