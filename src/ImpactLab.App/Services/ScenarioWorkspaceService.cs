using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Projects;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed class ScenarioWorkspaceService
{
    private readonly MaterialCatalog? _materials;
    private readonly MaterialProfileCatalog? _profiles;

    public ScenarioWorkspaceService(
        MaterialCatalog? materials = null,
        MaterialProfileCatalog? profiles = null
    )
    {
        _materials = materials;
        _profiles = profiles;
    }

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
        if (
            string.Equals(
                Path.GetExtension(path),
                ImpactLabProjectPackage.FileExtension,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            var cache = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ImpactLab",
                "project-cache"
            );
            var project = ImpactLabProjectPackage.Load(path, cache);
            foreach (var profile in project.Materials)
            {
                _materials?.ReplaceUser(profile.Definition, "Project material", profile.Notes);
                _profiles?.Upsert(profile);
            }
            Current = project.Scenario;
        }
        else
        {
            Current = ScenarioEnvelopeSerializer.Load(path);
        }
        CurrentPath = path;
        IsDirty = false;
    }

    public void Save(string? path = null)
    {
        var target =
            path ?? CurrentPath ?? throw new InvalidOperationException("Scenario has no path.");
        if (
            string.Equals(
                Path.GetExtension(target),
                ImpactLabProjectPackage.FileExtension,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            var customProfiles =
                _materials is null || _profiles is null
                    ? []
                    : _materials
                        .Entries.Where(x => !x.IsBuiltIn)
                        .Select(x => _profiles.Get(x.Definition.Id))
                        .ToArray();
            ImpactLabProjectPackage.Save(target, Current, customProfiles);
        }
        else
        {
            ScenarioEnvelopeSerializer.Save(target, Current);
        }
        CurrentPath = target;
        IsDirty = false;
    }

    public void MarkDirty() => IsDirty = true;
}
