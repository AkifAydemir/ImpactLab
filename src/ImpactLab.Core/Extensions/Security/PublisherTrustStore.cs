namespace ImpactLab.Core.Extensions.Security;

public sealed class PublisherTrustStore
{
    private readonly Dictionary<string, PublisherTrustRecord> _records = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Add(PublisherTrustRecord record) => _records[record.KeyId] = record;

    public PublisherTrustRecord? FindByKey(string keyId) => _records.GetValueOrDefault(keyId);

    public PublisherTrustRecord? FindByThumbprint(string? thumbprint) =>
        string.IsNullOrWhiteSpace(thumbprint)
            ? null
            : _records.Values.FirstOrDefault(x =>
                string.Equals(
                    x.CertificateThumbprint,
                    thumbprint,
                    StringComparison.OrdinalIgnoreCase
                )
            );

    public IReadOnlyList<PublisherTrustRecord> All =>
        _records.Values.OrderBy(x => x.Publisher).ToArray();
}
