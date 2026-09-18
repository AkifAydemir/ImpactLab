using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class WorkspaceMigrationV4ToV5 : IDocumentMigration
{
    public string DocumentType => "workspace";
    public int FromVersion => 4;
    public int ToVersion => 5;

    public JsonNode Migrate(JsonNode root)
    {
        root["payload"]!["resultStreaming"] ??= new JsonObject
        {
            { "cacheFrames", 12 },
            { "lod", "Auto" },
        };
        root["schemaVersion"] = 5;
        return root;
    }
}
