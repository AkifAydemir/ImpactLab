namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionHostBootstrap(
    int ProtocolVersion,
    string SessionId,
    string ExtensionId,
    string PackageDirectory,
    string EntryAssemblyPath,
    string? EntryType,
    ExtensionPermission GrantedPermissions,
    IpcCapability AllowedCapabilities,
    string KeyBase64,
    string WorkspaceRoot,
    string TempRoot
)
{
    public byte[] DecodeKey() => Convert.FromBase64String(KeyBase64);

    public void Validate()
    {
        if (ProtocolVersion != AuthenticatedIpcEnvelope.CurrentProtocolVersion)
            throw new InvalidOperationException("Unsupported extension-host protocol.");
        if (string.IsNullOrWhiteSpace(SessionId) || string.IsNullOrWhiteSpace(ExtensionId))
            throw new InvalidOperationException("Extension host session identity is required.");
        var dir = Path.GetFullPath(PackageDirectory);
        var entry = Path.GetFullPath(EntryAssemblyPath);
        if (
            !entry.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        )
            throw new InvalidOperationException("Entry assembly escapes package root.");
        if (DecodeKey().Length < 32)
            throw new InvalidOperationException("Extension host key is too short.");
    }
}
