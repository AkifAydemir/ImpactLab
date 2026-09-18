namespace ImpactLab.App.Accessibility;

public static class AccessibleSelectionNarrator
{
    public static string Describe(IReadOnlyCollection<string> ids, string? primary) =>
        ids.Count switch
        {
            0 => "No objects selected.",
            1 => $"Selected {primary ?? ids.First()}.",
            _ => $"{ids.Count} objects selected. Primary selection {primary ?? "not set"}.",
        };
}
