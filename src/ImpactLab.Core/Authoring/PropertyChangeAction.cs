namespace ImpactLab.Core.Authoring;

public sealed class PropertyChangeAction<T> : IUndoableAction
{
    private readonly Action<T> _setter;
    private readonly T _before;
    private T _after;
    private readonly string _mergeKey;

    public PropertyChangeAction(string name, string mergeKey, T before, T after, Action<T> setter)
    {
        Name = name;
        _mergeKey = mergeKey;
        _before = before;
        _after = after;
        _setter = setter;
    }

    public string Name { get; }

    public void Execute() => _setter(_after);

    public void Undo() => _setter(_before);

    public bool TryMerge(IUndoableAction newer)
    {
        if (newer is not PropertyChangeAction<T> n || n._mergeKey != _mergeKey)
            return false;
        _after = n._after;
        return true;
    }
}
