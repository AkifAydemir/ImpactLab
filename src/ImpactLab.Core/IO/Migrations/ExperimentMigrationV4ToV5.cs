using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class ExperimentMigrationV4ToV5 : IDocumentMigration
{
    public string DocumentType => "experiment";
    public int FromVersion => 4;
    public int ToVersion => 5;

    public JsonNode Migrate(JsonNode root)
    {
        root["payload"]!["multiObjective"] ??= false;
        root["payload"]!["saltelli"] ??= new JsonObject
        {
            { "baseSamples", 256 },
            { "includeSecondOrder", false },
        };
        root["schemaVersion"] = 5;
        return root;
    }
}
