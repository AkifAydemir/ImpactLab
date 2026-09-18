namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionManifest(
    string Id,
    string Name,
    string Version,
    string EntryAssembly,
    string EntryType,
    string ApiVersion,
    ExtensionPermission RequestedPermissions,
    string? Publisher,
    string? SignatureThumbprint,
    IReadOnlyList<string> Dependencies
);
