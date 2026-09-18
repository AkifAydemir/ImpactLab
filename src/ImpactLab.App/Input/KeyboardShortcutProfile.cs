using System.Windows.Input;

namespace ImpactLab.App.Input;

public sealed class KeyboardShortcutProfile
{
    public List<WorkspaceCommandBinding> Bindings { get; } =
    [
        new(WorkspaceCommandId.Save, Key.S, ModifierKeys.Control, "Save"),
        new(WorkspaceCommandId.Undo, Key.Z, ModifierKeys.Control, "Undo"),
        new(WorkspaceCommandId.Redo, Key.Y, ModifierKeys.Control, "Redo"),
        new(WorkspaceCommandId.Delete, Key.Delete, ModifierKeys.None, "Delete"),
        new(WorkspaceCommandId.FitView, Key.F, ModifierKeys.None, "Fit view"),
    ];

    public string? Resolve(Key key, ModifierKeys mods) =>
        Bindings.LastOrDefault(x => x.Key == key && x.Modifiers == mods)?.Id;
}
