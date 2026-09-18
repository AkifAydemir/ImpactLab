using ImpactLab.Core.Extensions.Sandbox;

namespace ImpactLab.Core.Extensions.Hosting;

public sealed record ExtensionHostProcessOptions(
    string HostExecutable,
    string WorkspaceRoot,
    string TempRoot,
    ExtensionSandboxPolicy SandboxPolicy,
    TimeSpan RequestTimeout = default
)
{
    public TimeSpan EffectiveRequestTimeout =>
        RequestTimeout == default ? TimeSpan.FromSeconds(30) : RequestTimeout;
}
