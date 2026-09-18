namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionTrustDecision(
    ExtensionTrustLevel Level,
    bool SignatureValid,
    string Reason,
    string? PublisherThumbprint = null
);
