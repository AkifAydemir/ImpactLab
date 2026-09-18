using ImpactLab.Core.Authoring;
using ImpactLab.Core.IO;

namespace ImpactLab.App.Services;

public sealed class WorkspaceSessionService
{
    public WorkspaceDocument Document { get; private set; } = new();
    public string? Path { get; private set; }
    public bool IsDirty { get; private set; }
    public UndoRedoStack UndoRedo { get; } = new();
    public event EventHandler? Changed;

    public void New()
    {
        Document = new WorkspaceDocument();
        Path = null;
        IsDirty = false;
        UndoRedo.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Open(string path)
    {
        Document = WorkspaceSerializer.Load(path);
        Path = path;
        IsDirty = false;
        UndoRedo.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Save(string? path = null)
    {
        var p = path ?? Path ?? throw new InvalidOperationException("Workspace has no path.");
        WorkspaceSerializer.Save(p, Document);
        Path = p;
        IsDirty = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void MarkDirty()
    {
        IsDirty = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(IUndoableAction action)
    {
        UndoRedo.Execute(action);
        MarkDirty();
    }
}
