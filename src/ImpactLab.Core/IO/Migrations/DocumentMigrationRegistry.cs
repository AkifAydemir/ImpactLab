using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class DocumentMigrationRegistry
{
    private readonly List<IDocumentMigration> _steps = [];

    public void Register(IDocumentMigration migration) => _steps.Add(migration);

    public JsonNode Migrate(
        string documentType,
        int sourceVersion,
        int targetVersion,
        JsonNode root
    )
    {
        if (targetVersion < sourceVersion)
            throw new NotSupportedException("Downgrade migrations are not supported.");
        var current = sourceVersion;
        var value = root;
        while (current < targetVersion)
        {
            var step =
                _steps.SingleOrDefault(x =>
                    string.Equals(x.DocumentType, documentType, StringComparison.OrdinalIgnoreCase)
                    && x.FromVersion == current
                )
                ?? throw new NotSupportedException(
                    $"No {documentType} migration from v{current} toward v{targetVersion}."
                );
            if (step.ToVersion <= current)
                throw new InvalidOperationException(
                    $"Migration {step.GetType().Name} does not advance schema version."
                );
            value = step.Migrate(value);
            current = step.ToVersion;
        }
        if (current != targetVersion)
            throw new InvalidOperationException(
                $"Migration chain ended at v{current}, expected v{targetVersion}."
            );
        return value;
    }

    public static DocumentMigrationRegistry CreateDefault()
    {
        var r = new DocumentMigrationRegistry();
        r.Register(new ScenarioMigrationV3ToV4());
        r.Register(new ScenarioMigrationV4ToV5());
        r.Register(new ExperimentMigrationV3ToV4());
        r.Register(new ExperimentMigrationV4ToV5());
        r.Register(new WorkspaceMigrationV3ToV4());
        r.Register(new WorkspaceMigrationV4ToV5());
        return r;
    }
}
