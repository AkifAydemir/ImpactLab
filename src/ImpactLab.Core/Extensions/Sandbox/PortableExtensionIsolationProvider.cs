using System.Diagnostics;

namespace ImpactLab.Core.Extensions.Sandbox;

public sealed class PortableExtensionIsolationProvider : IExtensionIsolationProvider
{
    public string Name => "portable-process";
    public bool IsAvailable => true;

    public IExtensionIsolationLease Attach(Process process, ExtensionSandboxPolicy policy) =>
        new Lease([
            "Separate child process enabled.",
            "No platform restricted-token/container primitive is claimed by the portable provider.",
        ]);

    private sealed class Lease : IExtensionIsolationLease
    {
        public Lease(IReadOnlyList<string> d) => Diagnostics = d;

        public string Provider => "portable-process";
        public IReadOnlyList<string> Diagnostics { get; }

        public void Dispose() { }
    }
}
