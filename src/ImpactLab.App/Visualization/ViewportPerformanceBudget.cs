namespace ImpactLab.App.Visualization;

public sealed record ViewportPerformanceBudget(
    int MaxVisibleTriangles = 2_000_000,
    int MaxGlyphs = 100_000,
    int MaxLabels = 500,
    TimeSpan TargetFrameTime = default
)
{
    public TimeSpan EffectiveTarget =>
        TargetFrameTime == default ? TimeSpan.FromMilliseconds(16.7) : TargetFrameTime;
}
