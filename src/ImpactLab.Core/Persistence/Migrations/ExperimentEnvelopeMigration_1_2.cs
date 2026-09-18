using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class ExperimentEnvelopeMigration_1_2 : ISchemaMigration
{
    public string DocumentType => "experiment";
    public int FromVersion => 1;
    public int ToVersion => 2;

    public JsonObject Migrate(JsonObject root)
    {
        root["schemaVersion"] = 2;
        root["execution"] ??= new JsonObject
        {
            ["maxParallelism"] = 1,
            ["checkpointEnabled"] = true,
        };
        return root;
    }
}
