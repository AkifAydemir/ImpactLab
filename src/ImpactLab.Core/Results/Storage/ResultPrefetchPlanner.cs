namespace ImpactLab.Core.Results.Storage;

public static class ResultPrefetchPlanner
{
    public static ResultPrefetchPlan Around(
        string fieldId,
        int centerFrame,
        IReadOnlyList<int> availableFrames,
        int radius = 2
    )
    {
        var items = availableFrames
            .Select(f => new { Frame = f, Distance = Math.Abs(f - centerFrame) })
            .Where(x => x.Distance <= Math.Max(0, radius))
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Frame)
            .Select((x, i) => new ResultPrefetchItem(fieldId, x.Frame, i))
            .ToArray();
        return new(items);
    }
}
