using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions;

public sealed record ExtensionPackageManifest(
    string Id,
    string DisplayName,
    string Version,
    string EntryAssembly,
    string? EntryType,
    string MinimumImpactLabVersion = "9.0.0",
    string[]? Capabilities = null,
    string ApiVersion = "1.0",
    ExtensionPermission RequestedPermissions = ExtensionPermission.ReadProject,
    string? Publisher = null,
    string? SignatureThumbprint = null,
    string[]? Dependencies = null,
    bool PreferIsolation = true
)
{
    public Security.ExtensionManifest ToSecurityManifest() =>
        new(
            Id,
            DisplayName,
            Version,
            EntryAssembly,
            EntryType ?? string.Empty,
            ApiVersion,
            RequestedPermissions,
            Publisher,
            SignatureThumbprint,
            Dependencies ?? []
        );
}
