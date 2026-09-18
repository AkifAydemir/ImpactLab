using ImpactLab.Core.IO;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed class ScenarioWorkspaceService
{
    public string? CurrentPath { get; private set; }
    public ScenarioDefinition Current { get; private set; } = new();
    public bool IsDirty { get; private set; }

    public void New()
    {
        Current = new ScenarioDefinition();
        CurrentPath = null;
        IsDirty = false;
    }

    public void Use(ScenarioDefinition scenario)
    {
        Current = scenario ?? throw new ArgumentNullException(nameof(scenario));
        CurrentPath = null;
        IsDirty = false;
    }

    public void Open(string path)
    {
        Current = ScenarioEnvelopeSerializer.Load(path);
        CurrentPath = path;
        IsDirty = false;
    }

    public void Save(string? path = null)
    {
        var target =
            path ?? CurrentPath ?? throw new InvalidOperationException("Scenario has no path.");
        ScenarioEnvelopeSerializer.Save(target, Current);
        CurrentPath = target;
        IsDirty = false;
    }

    public void MarkDirty() => IsDirty = true;
}
