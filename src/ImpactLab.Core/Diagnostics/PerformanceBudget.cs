namespace ImpactLab.Core.Diagnostics;

public sealed record PerformanceBudget(
    PerformanceStage Stage,
    TimeSpan SoftLimit,
    TimeSpan? HardLimit = null
)
{
    public void Validate()
    {
        if (SoftLimit <= TimeSpan.Zero)
            throw new InvalidOperationException("Soft limit must be positive.");
        if (HardLimit is not null && HardLimit <= SoftLimit)
            throw new InvalidOperationException("Hard limit must exceed soft limit.");
    }
}
