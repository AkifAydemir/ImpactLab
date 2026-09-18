namespace ImpactLab.Core.Results.Storage;

public static class ResultStoreQueryEngine
{
    public static IReadOnlyList<ResultStoreRunIndex> Query(
        ResultStoreCatalogSnapshot snapshot,
        ResultStoreQuery query
    )
    {
        query.Validate();
        IEnumerable<ResultStoreRunIndex> rows = snapshot.Runs;
        if (!string.IsNullOrWhiteSpace(query.ScenarioFingerprint))
            rows = rows.Where(x =>
                string.Equals(
                    x.ScenarioFingerprint,
                    query.ScenarioFingerprint,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        if (!string.IsNullOrWhiteSpace(query.RequiredField))
            rows = rows.Where(x =>
                x.Fields.Contains(query.RequiredField, StringComparer.OrdinalIgnoreCase)
            );
        if (query.CreatedAfterUtc is { } after)
            rows = rows.Where(x => x.CreatedUtc >= after);
        if (query.CreatedBeforeUtc is { } before)
            rows = rows.Where(x => x.CreatedUtc <= before);
        if (query.MinimumBytes is { } bytes)
            rows = rows.Where(x => x.TotalBytes >= bytes);
        return rows.OrderByDescending(x => x.CreatedUtc).Take(query.Limit).ToArray();
    }
}
