namespace ImpactLab.Core.Extensions.Security;

public sealed class PublisherChainPolicy
{
    private readonly PublisherTrustStore _store;

    public PublisherChainPolicy(PublisherTrustStore store) => _store = store;

    public ExtensionTrustDecision Evaluate(
        ExtensionManifest manifest,
        ExtensionTrustDecision signatureDecision
    )
    {
        if (!signatureDecision.SignatureValid)
            return signatureDecision with
            {
                Level = ExtensionTrustLevel.Untrusted,
                Reason = "Publisher signature is not trusted.",
            };
        var record = _store.FindByThumbprint(manifest.SignatureThumbprint);
        if (record is null || !record.Enabled)
            return new(
                ExtensionTrustLevel.Untrusted,
                false,
                "Publisher certificate is not in the trusted publisher store.",
                manifest.SignatureThumbprint
            );
        return new(
            record.Level,
            true,
            $"Trusted publisher: {record.Publisher}.",
            record.CertificateThumbprint
        );
    }
}
