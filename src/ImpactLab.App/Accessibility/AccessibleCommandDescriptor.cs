namespace ImpactLab.App.Accessibility;

public sealed record AccessibleCommandDescriptor(
    string CommandId,
    string Name,
    string Description,
    string? Shortcut,
    string AutomationCategory
);
