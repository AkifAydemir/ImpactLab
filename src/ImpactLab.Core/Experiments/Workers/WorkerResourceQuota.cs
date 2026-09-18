namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerResourceQuota(
    TimeSpan WallTime,
    int MaxWorkingSetMb = 4096,
    int MaxThreads = 0
)
{
    public static WorkerResourceQuota Default => new(TimeSpan.FromMinutes(30));

    public void Validate()
    {
        if (WallTime <= TimeSpan.Zero)
            throw new InvalidOperationException("Worker wall time must be positive.");
        if (MaxWorkingSetMb < 128)
            throw new InvalidOperationException(
                "Worker working-set quota must be at least 128 MB."
            );
        if (MaxThreads < 0)
            throw new InvalidOperationException("Worker thread quota cannot be negative.");
    }
}
