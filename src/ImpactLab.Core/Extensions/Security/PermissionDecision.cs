namespace ImpactLab.Core.Extensions.Security;

public sealed record PermissionDecision(
    ExtensionPermission Requested,
    ExtensionPermission Granted,
    IReadOnlyList<ExtensionPermission> Denied
);
