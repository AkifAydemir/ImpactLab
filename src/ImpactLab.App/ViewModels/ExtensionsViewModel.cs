using ImpactLab.App.Services;
using ImpactLab.Core.Extensions;

namespace ImpactLab.App.ViewModels;

public sealed class ExtensionsViewModel
{
    private readonly ExtensionHostService _service;

    public ExtensionsViewModel(ExtensionHostService? service = null) => _service = service ?? new();

    public IReadOnlyList<ExtensionPackage> Discovered { get; private set; } = [];
    public IReadOnlyList<ExtensionHandle> Loaded => _service.LoadedInProcess;
    public string Status { get; private set; } = "Idle";

    public void Scan(string root)
    {
        var r = _service.Discover(root);
        Discovered = r.Packages;
        Status = $"{r.Packages.Count} extension package(s) found; {r.Failures.Count} rejected.";
    }

    public void Load(ExtensionPackage package)
    {
        _service.LoadTrustedInProcess(package);
        Status = $"Loaded trusted extension {package.Manifest.DisplayName}";
    }

    public void Unload(string id)
    {
        _service.UnloadInProcess(id);
        Status = $"Unloaded {id}";
    }
}
