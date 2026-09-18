namespace ImpactLab.Core.Experiments;

public sealed record ExperimentCheckpoint(
    string ExperimentName,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ExperimentRunResult> CompletedRuns,
    IReadOnlyList<int> CompletedPointIndices
)
{
    public bool Contains(int index) => CompletedPointIndices.Contains(index);
}
