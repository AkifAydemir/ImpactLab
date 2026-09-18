namespace ImpactLab.Core.Extensions.Security;

public sealed record PublisherTrustRecord(
    string Publisher,
    string KeyId,
    string? CertificateThumbprint,
    ExtensionTrustLevel Level,
    ExtensionPermission MaximumPermissions,
    bool Enabled = true
);
