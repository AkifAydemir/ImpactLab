using System.ComponentModel;
using ImpactLab.App.Services;
using ImpactLab.Core.Extensions;
using ImpactLab.Core.Extensions.Hosting;

namespace ImpactLab.App.ViewModels;

public sealed class ExtensionsWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly ExtensionHostService _service;
    private IReadOnlyList<ExtensionPackage> _packages = [];
    private IReadOnlyList<ExtensionLoadFailure> _failures = [];
    private string _status = "Idle";

    public ExtensionsWorkspaceViewModel(ExtensionHostService service) => _service = service;

    public IReadOnlyList<ExtensionPackage> Packages
    {
        get => _packages;
        private set
        {
            _packages = value;
            Changed(nameof(Packages));
        }
    }
    public IReadOnlyList<ExtensionLoadFailure> Failures
    {
        get => _failures;
        private set
        {
            _failures = value;
            Changed(nameof(Failures));
        }
    }
    public IReadOnlyList<ExtensionHandle> LoadedInProcess => _service.LoadedInProcess;
    public IReadOnlyList<ExtensionHostProcessSession> Isolated => _service.Isolated;
    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            Changed(nameof(Status));
        }
    }

    public void Scan(string root)
    {
        var r = _service.Discover(root);
        Packages = r.Packages;
        Failures = r.Failures;
        Status = $"{Packages.Count} package(s), {Failures.Count} diagnostic(s)";
    }

    public ExtensionLoadPlan Inspect(ExtensionPackage package) => _service.Plan(package);

    public async Task LaunchIsolatedAsync(
        ExtensionPackage package,
        ExtensionHostProcessOptions options,
        CancellationToken ct = default
    )
    {
        var s = await _service.LaunchIsolatedAsync(package, options, ct);
        Status =
            $"Isolated host started for {s.ExtensionId} ({string.Join("; ", s.IsolationDiagnostics)})";
        Changed(nameof(Isolated));
    }

    public void LoadTrustedInProcess(ExtensionPackage package)
    {
        _service.LoadTrustedInProcess(package);
        Status = $"Trusted in-process extension loaded: {package.Manifest.DisplayName}";
        Changed(nameof(LoadedInProcess));
    }

    public async Task UnloadAsync(string id)
    {
        _service.UnloadInProcess(id);
        await _service.ShutdownIsolatedAsync(id);
        Status = $"Unloaded {id}";
        Changed(nameof(LoadedInProcess));
        Changed(nameof(Isolated));
    }

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}
