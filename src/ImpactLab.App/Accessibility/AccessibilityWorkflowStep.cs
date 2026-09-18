namespace ImpactLab.App.Accessibility;

public sealed record AccessibilityWorkflowStep(
    string Id,
    string Name,
    string AutomationName,
    string? Shortcut,
    bool Required = true
);
