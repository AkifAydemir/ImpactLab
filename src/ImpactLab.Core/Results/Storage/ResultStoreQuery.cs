namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreQuery(
    string? ScenarioFingerprint = null,
    string? RequiredField = null,
    DateTimeOffset? CreatedAfterUtc = null,
    DateTimeOffset? CreatedBeforeUtc = null,
    long? MinimumBytes = null,
    int Limit = 100
)
{
    public void Validate()
    {
        if (Limit < 1 || Limit > 10000)
            throw new InvalidOperationException("Result query limit must be 1..10000.");
    }
}
