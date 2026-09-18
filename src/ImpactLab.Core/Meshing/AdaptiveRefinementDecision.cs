namespace ImpactLab.Core.Meshing;

public readonly record struct AdaptiveRefinementDecision(bool Refine, int Priority, string Reason)
{
    public static AdaptiveRefinementDecision No => new(false, 0, "");

    public static AdaptiveRefinementDecision Yes(int priority, string reason) =>
        new(true, priority, reason);
}
