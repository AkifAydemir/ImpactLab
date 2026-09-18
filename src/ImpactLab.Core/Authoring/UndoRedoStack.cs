namespace ImpactLab.Core.Authoring;

public sealed class UndoRedoStack
{
    private readonly Stack<IUndoableAction> _undo = [];
    private readonly Stack<IUndoableAction> _redo = [];
    public int Capacity { get; init; } = 200;
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public string? UndoName => _undo.TryPeek(out var a) ? a.Name : null;
    public string? RedoName => _redo.TryPeek(out var a) ? a.Name : null;

    public void Execute(IUndoableAction action)
    {
        action.Execute();
        if (_undo.TryPeek(out var last) && last.TryMerge(action)) { }
        else
        {
            _undo.Push(action);
            Trim();
        }
        _redo.Clear();
    }

    public void Undo()
    {
        if (!_undo.TryPop(out var a))
            return;
        a.Undo();
        _redo.Push(a);
    }

    public void Redo()
    {
        if (!_redo.TryPop(out var a))
            return;
        a.Execute();
        _undo.Push(a);
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    private void Trim()
    {
        if (_undo.Count <= Capacity)
            return;
        var keep = _undo.Reverse().Skip(_undo.Count - Capacity).ToArray();
        _undo.Clear();
        foreach (var a in keep)
            _undo.Push(a);
    }
}
