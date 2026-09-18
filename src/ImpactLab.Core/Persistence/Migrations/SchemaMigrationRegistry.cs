using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class SchemaMigrationRegistry
{
    private readonly List<ISchemaMigration> _steps = [];

    public void Register(ISchemaMigration migration) => _steps.Add(migration);

    public JsonObject Migrate(string type, int sourceVersion, int targetVersion, JsonObject root)
    {
        var current = sourceVersion;
        while (current < targetVersion)
        {
            var step =
                _steps.SingleOrDefault(x =>
                    string.Equals(x.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                    && x.FromVersion == current
                )
                ?? throw new NotSupportedException(
                    $"Missing envelope migration {type} v{current} -> v{targetVersion}."
                );
            root = step.Migrate(root);
            current = step.ToVersion;
        }
        if (current != targetVersion)
            throw new InvalidOperationException(
                $"Envelope migration ended at v{current}, expected v{targetVersion}."
            );
        return root;
    }

    public static SchemaMigrationRegistry CreateDefault()
    {
        var r = new SchemaMigrationRegistry();
        foreach (var step in V10MigrationRegistry.Create())
            r.Register(step);
        return r;
    }
}
