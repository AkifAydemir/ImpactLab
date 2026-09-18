namespace ImpactLab.App.Layout;

public static class WorkspaceLayoutValidator
{
    public static IReadOnlyList<string> Validate(WorkspaceLayoutState state)
    {
        var e = new List<string>();
        if (
            state.Panes.Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != state.Panes.Count
        )
            e.Add("Duplicate pane ids.");
        if (state.Panes.Count == 0)
            e.Add("At least one pane is required.");
        return e;
    }
}
