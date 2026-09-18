namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionSignatureEnvelope(
    string Algorithm,
    string KeyId,
    string PayloadSha256,
    string SignatureBase64,
    DateTimeOffset SignedUtc
);
