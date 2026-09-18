namespace ImpactLab.App.Accessibility;

public static class AccessibilityWorkflowCatalog
{
    public static IReadOnlyList<AccessibilityWorkflowStep> Default { get; } =
    [
        new("workspace.tabs", "Workspace navigation", "Workspace navigation", "Ctrl+Tab"),
        new("viewport.select", "Viewport selection", "Engineering viewport", "Enter", true),
        new("results.field", "Result field selection", "Result field", "Alt+F", true),
        new("results.frame", "Timeline frame navigation", "Result frame", "Left/Right", true),
        new(
            "extensions.scan",
            "Extension package scan",
            "Extension package catalogue",
            null,
            false
        ),
        new("reports.export", "Report export", "Engineering report export", null, false),
    ];
}
