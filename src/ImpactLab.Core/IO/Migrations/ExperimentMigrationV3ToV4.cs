using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class ExperimentMigrationV3ToV4 : IDocumentMigration
{
    public string DocumentType => "experiment";
    public int FromVersion => 3;
    public int ToVersion => 4;

    public JsonNode Migrate(JsonNode root)
    {
        var p = root["payload"]!.AsObject();
        p["sensitivityMethod"] ??= "None";
        p["optimizerMethod"] ??= "None";
        root["schemaVersion"] = 4;
        return root;
    }
}
