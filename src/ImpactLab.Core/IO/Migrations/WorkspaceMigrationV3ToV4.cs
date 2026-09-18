using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class WorkspaceMigrationV3ToV4 : IDocumentMigration
{
    public string DocumentType => "workspace";
    public int FromVersion => 3;
    public int ToVersion => 4;

    public JsonNode Migrate(JsonNode root)
    {
        var p = root["payload"]!.AsObject();
        p["layout"] ??= new JsonObject
        {
            { "name", "Default" },
            { "activeWorkspaceId", "simulation" },
        };
        p["resultFieldId"] ??= "displacement";
        root["schemaVersion"] = 4;
        return root;
    }
}
