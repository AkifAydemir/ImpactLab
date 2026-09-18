namespace ImpactLab.App.Visualization;

public sealed class AdaptiveRenderBudgetController
{
    public ViewportQualityLevel Current { get; private set; } = ViewportQualityLevel.High;

    public void Observe(TimeSpan frame, ViewportPerformanceBudget b)
    {
        if (frame > b.EffectiveTarget * 2)
            Current = ViewportQualityLevel.Interactive;
        else if (frame > b.EffectiveTarget * 1.25)
            Current = ViewportQualityLevel.Medium;
        else
            Current = ViewportQualityLevel.High;
    }
}
