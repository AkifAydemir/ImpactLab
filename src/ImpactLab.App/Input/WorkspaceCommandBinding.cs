using System.Windows.Input;

namespace ImpactLab.App.Input;

public sealed record WorkspaceCommandBinding(
    string Id,
    Key Key,
    ModifierKeys Modifiers,
    string DisplayName
);
