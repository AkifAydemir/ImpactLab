namespace ImpactLab.Core.Experiments;

public sealed record ExperimentResult(
    string Name,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<ExperimentRunResult> Runs
)
{
    public int SuccessCount => Runs.Count(x => x.Succeeded);
    public int FailureCount => Runs.Count - SuccessCount;
    public TimeSpan Elapsed => CompletedAt - StartedAt;
}
