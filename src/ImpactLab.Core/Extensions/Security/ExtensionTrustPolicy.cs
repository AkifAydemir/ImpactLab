namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionTrustPolicy(
    ExtensionTrustLevel MinimumLevel = ExtensionTrustLevel.Untrusted,
    ExtensionPermission MaximumPermissions =
        ExtensionPermission.ReadProject | ExtensionPermission.ReadFiles,
    bool RequireSignature = false,
    bool AllowNativeCode = false
)
{
    public void Validate(ExtensionManifest m, ExtensionTrustDecision d)
    {
        if (d.Level < MinimumLevel)
            throw new UnauthorizedAccessException("Extension trust level is insufficient.");
        if ((m.RequestedPermissions & ~MaximumPermissions) != 0)
            throw new UnauthorizedAccessException("Extension requests disallowed permissions.");
        if (!AllowNativeCode && m.RequestedPermissions.HasFlag(ExtensionPermission.NativeCode))
            throw new UnauthorizedAccessException("Native extension code disabled.");
        if (RequireSignature && !d.SignatureValid)
            throw new UnauthorizedAccessException("Valid extension signature required.");
    }
}
