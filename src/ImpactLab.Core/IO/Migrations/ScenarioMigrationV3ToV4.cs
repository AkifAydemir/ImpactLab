using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class ScenarioMigrationV3ToV4 : IDocumentMigration
{
    public string DocumentType => "scenario";
    public int FromVersion => 3;
    public int ToVersion => 4;

    public JsonNode Migrate(JsonNode root)
    {
        var p = root["payload"]!.AsObject();
        p["continuum"] ??= new JsonObject
        {
            { "mode", "LinearStatic" },
            { "enableSelfContact", false },
            {
                "implicitNonlinear",
                new JsonObject
                {
                    { "timeStepSeconds", 0.0001 },
                    { "durationSeconds", 0.02 },
                    { "maxCutbacks", 6 },
                }
            },
        };
        root["schemaVersion"] = 4;
        return root;
    }
}
