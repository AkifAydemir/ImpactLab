using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class WorkspaceEnvelopeMigration_2_3 : ISchemaMigration
{
    public string DocumentType => "workspace";
    public int FromVersion => 2;
    public int ToVersion => 3;

    public JsonObject Migrate(JsonObject root)
    {
        root["schemaVersion"] = 3;
        var p = root["payload"]?.AsObject() ?? root;
        if (p["keyboardProfile"] is null)
            p["keyboardProfile"] = "default";
        if (p["reportTemplateId"] is null)
            p["reportTemplateId"] = "engineering-standard";
        return root;
    }
}
