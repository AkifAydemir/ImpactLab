namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionAuditEvent(
    DateTimeOffset TimeUtc,
    string ExtensionId,
    string Action,
    bool Allowed,
    string Detail
);
