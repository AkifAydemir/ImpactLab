namespace ImpactLab.Core.Experiments;

public sealed record ExperimentExecutionPolicy(
    int MaxDegreeOfParallelism = 2,
    bool ContinueOnFailure = true,
    bool ResumeFromCheckpoint = true,
    int CheckpointEveryRuns = 1
)
{
    public void Validate()
    {
        if (MaxDegreeOfParallelism <= 0 || MaxDegreeOfParallelism > 64)
            throw new InvalidOperationException("Parallelism must be in 1..64.");
        if (CheckpointEveryRuns <= 0)
            throw new InvalidOperationException("Checkpoint cadence must be positive.");
    }
}
