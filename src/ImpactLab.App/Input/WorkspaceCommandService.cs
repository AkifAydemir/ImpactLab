namespace ImpactLab.App.Input;

public sealed class WorkspaceCommandService
{
    private readonly Dictionary<string, Func<bool>> _can = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Action> _run = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string id, Action execute, Func<bool>? canExecute = null)
    {
        _run[id] = execute;
        _can[id] = canExecute ?? (() => true);
    }

    public bool CanExecute(string id) =>
        _run.ContainsKey(id) && (!_can.TryGetValue(id, out var c) || c());

    public bool Execute(string id)
    {
        if (!CanExecute(id))
            return false;
        _run[id]();
        return true;
    }

    public IReadOnlyList<string> Commands => _run.Keys.OrderBy(x => x).ToArray();
}
