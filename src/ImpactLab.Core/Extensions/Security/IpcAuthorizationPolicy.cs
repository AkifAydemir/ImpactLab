namespace ImpactLab.Core.Extensions.Security;

public sealed record IpcAuthorizationPolicy(IpcCapability Allowed, string ExtensionId)
{
    public bool Allows(IpcCapability c) => (Allowed & c) == c;
}
