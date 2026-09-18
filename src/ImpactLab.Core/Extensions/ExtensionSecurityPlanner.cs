using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions;

public sealed class ExtensionSecurityPlanner
{
    private readonly ExtensionTrustPolicy _policy;
    private readonly IExtensionSignatureVerifier _verifier;
    private readonly PublisherChainPolicy? _publishers;

    public ExtensionSecurityPlanner(
        ExtensionTrustPolicy policy,
        IExtensionSignatureVerifier verifier,
        PublisherChainPolicy? publishers = null
    )
    {
        _policy = policy;
        _verifier = verifier;
        _publishers = publishers;
    }

    public ExtensionLoadPlan Plan(ExtensionPackage package)
    {
        var manifest = package.Manifest.ToSecurityManifest();
        var trust = _verifier.Verify(package.EntryAssemblyPath, manifest);
        if (_publishers is not null && trust.SignatureValid)
            trust = _publishers.Evaluate(manifest, trust);
        _policy.Validate(manifest, trust);
        var permissions = PermissionEvaluator.Evaluate(
            manifest.RequestedPermissions,
            _policy.MaximumPermissions
        );
        var mode =
            !package.Manifest.PreferIsolation && trust.Level >= ExtensionTrustLevel.FirstParty
                ? ExtensionExecutionMode.TrustedInProcess
                : ExtensionExecutionMode.IsolatedProcess;
        var ipc = new IpcAuthorizationPolicy(
            ToCapabilities(package.Manifest.Capabilities),
            manifest.Id
        );
        return new(package, mode, trust, permissions, ipc);
    }

    private static IpcCapability ToCapabilities(string[]? caps)
    {
        var c = IpcCapability.None;
        foreach (var raw in caps ?? [])
            if (Enum.TryParse<IpcCapability>(raw, true, out var parsed))
                c |= parsed;
        return c == IpcCapability.None ? IpcCapability.ReadScenario | IpcCapability.ReadResults : c;
    }
}
