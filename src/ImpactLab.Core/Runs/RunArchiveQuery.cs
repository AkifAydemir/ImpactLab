namespace ImpactLab.Core.Runs;

public sealed record RunArchiveQuery(
    string? Text = null,
    string? Tag = null,
    string? ScenarioHash = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null
)
{
    public IEnumerable<ArchivedRunRecord> Apply(IEnumerable<ArchivedRunRecord> source)
    {
        var q = source;
        if (!string.IsNullOrWhiteSpace(Text))
            q = q.Where(x =>
                x.Name.Contains(Text, StringComparison.OrdinalIgnoreCase)
                || x.Annotations.Any(a => a.Text.Contains(Text, StringComparison.OrdinalIgnoreCase))
            );
        if (!string.IsNullOrWhiteSpace(Tag))
            q = q.Where(x => x.Tags.Contains(Tag, StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(ScenarioHash))
            q = q.Where(x =>
                string.Equals(x.ScenarioHash, ScenarioHash, StringComparison.OrdinalIgnoreCase)
            );
        if (FromUtc is not null)
            q = q.Where(x => x.CreatedUtc >= FromUtc);
        if (ToUtc is not null)
            q = q.Where(x => x.CreatedUtc <= ToUtc);
        return q.OrderByDescending(x => x.CreatedUtc);
    }
}
