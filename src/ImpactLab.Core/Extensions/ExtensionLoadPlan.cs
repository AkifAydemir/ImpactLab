using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions;

public sealed record ExtensionLoadPlan(
    ExtensionPackage Package,
    ExtensionExecutionMode Mode,
    ExtensionTrustDecision Trust,
    PermissionDecision Permissions,
    IpcAuthorizationPolicy IpcAuthorization
);
