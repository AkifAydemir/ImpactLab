using System.Windows.Input;

namespace ImpactLab.App.Input;

public sealed class WorkspaceCommandRouter
{
    private readonly Dictionary<(Key Key, ModifierKeys Mods), Action> _bindings = [];

    public void Bind(Key key, ModifierKeys modifiers, Action action) =>
        _bindings[(key, modifiers)] = action;

    public bool TryExecute(KeyEventArgs e)
    {
        if (!_bindings.TryGetValue((e.Key, Keyboard.Modifiers), out var action))
            return false;
        action();
        e.Handled = true;
        return true;
    }
}
