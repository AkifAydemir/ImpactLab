namespace ImpactLab.Core.Extensions.Security;

public sealed record PublisherIdentity(
    string Name,
    string KeyId,
    string? Organization,
    string? CertificateThumbprint
);
