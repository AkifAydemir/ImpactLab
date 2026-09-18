namespace ImpactLab.Core.Persistence;

public sealed record SchemaCompatibilityResult(string Id, bool Passed, string Message);

public sealed class SchemaCompatibilitySuite
{
    public IReadOnlyList<SchemaCompatibilityResult> Run(
        IEnumerable<SchemaCompatibilityCase> cases,
        Func<string, int, int, string> migrate
    )
    {
        var results = new List<SchemaCompatibilityResult>();
        foreach (var c in cases)
        {
            try
            {
                var source = File.ReadAllText(c.FixturePath);
                var migrated = migrate(source, c.FromVersion, c.ToVersion);
                results.Add(
                    new(c.Id, !string.IsNullOrWhiteSpace(migrated), "Migration produced payload.")
                );
            }
            catch (Exception ex)
            {
                results.Add(new(c.Id, false, ex.Message));
            }
        }
        return results;
    }
}
