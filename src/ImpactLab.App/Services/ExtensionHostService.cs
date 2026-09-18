using ImpactLab.Core.Backends;
using ImpactLab.Core.Extensions;
using ImpactLab.Core.Extensions.Hosting;
using ImpactLab.Core.Extensions.Sandbox;
using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.App.Services;

public sealed class ExtensionHostService
{
    private readonly ExtensionContext _context;
    private readonly ExtensionHost _inProcess;
    private readonly ExtensionSecurityPlanner _planner;
    private readonly ExtensionProcessLauncher _launcher;
    private readonly List<ExtensionHostProcessSession> _isolated = [];

    public ExtensionHostService(
        SimulationBackendRegistry? backends = null,
        ExtensionTrustPolicy? trustPolicy = null,
        IExtensionSignatureVerifier? verifier = null,
        PublisherChainPolicy? publishers = null,
        IExtensionIsolationProvider? isolation = null
    )
    {
        _context = new(backends ?? SimulationBackendRegistry.CreateDefault());
        _inProcess = new(_context);
        _planner = new(
            trustPolicy ?? new ExtensionTrustPolicy(),
            verifier ?? new HashAllowListVerifier([]),
            publishers
        );
        _launcher = new(isolation);
    }

    public IReadOnlyList<ExtensionHandle> LoadedInProcess => _inProcess.Loaded;
    public IReadOnlyList<ExtensionHostProcessSession> Isolated => _isolated;

    public ExtensionDiscoveryResult Discover(string root) =>
        new ExtensionDiscoveryService().Discover(root);

    public ExtensionLoadPlan Plan(ExtensionPackage package) => _planner.Plan(package);

    public ExtensionHandle LoadTrustedInProcess(
        ExtensionPackage package,
        Version? hostVersion = null
    )
    {
        var plan = Plan(package);
        if (plan.Mode != ExtensionExecutionMode.TrustedInProcess)
            throw new UnauthorizedAccessException(
                "Extension security plan requires isolated execution."
            );
        return _inProcess.Load(package, hostVersion ?? new Version(16, 0, 0));
    }

    public async Task<ExtensionHostProcessSession> LaunchIsolatedAsync(
        ExtensionPackage package,
        ExtensionHostProcessOptions options,
        CancellationToken ct = default
    )
    {
        var plan = Plan(package);
        if (plan.Mode != ExtensionExecutionMode.IsolatedProcess)
            throw new InvalidOperationException("Extension plan is trusted in-process mode.");
        var session = await _launcher.LaunchAsync(plan, options, ct);
        _isolated.Add(session);
        return session;
    }

    public void UnloadInProcess(string id) => _inProcess.Unload(id);

    public async Task ShutdownIsolatedAsync(string id)
    {
        var s = _isolated.FirstOrDefault(x =>
            x.ExtensionId.Equals(id, StringComparison.OrdinalIgnoreCase)
        );
        if (s is null)
            return;
        _isolated.Remove(s);
        await s.DisposeAsync();
    }
}
