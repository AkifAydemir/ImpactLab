using ImpactLab.Core.Projects;

namespace ImpactLab.App.Services;

public sealed class ProjectWorkspaceService
{
    public string? CurrentPath { get; private set; }
    public SimulationProject Current { get; private set; } = new();
    public bool IsDirty { get; private set; }

    public void NewProject()
    {
        Current = new SimulationProject();
        CurrentPath = null;
        IsDirty = false;
    }

    public void Open(string path)
    {
        Current = ProjectJsonSerializer.Load(path);
        CurrentPath = path;
        IsDirty = false;
    }

    public void Save(string? path = null)
    {
        var target =
            path ?? CurrentPath ?? throw new InvalidOperationException("Project has no path.");
        ProjectJsonSerializer.Save(target, Current);
        CurrentPath = target;
        IsDirty = false;
    }

    public void MarkDirty() => IsDirty = true;
}
