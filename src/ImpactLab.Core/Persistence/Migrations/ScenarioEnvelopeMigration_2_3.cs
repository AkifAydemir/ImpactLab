using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class ScenarioEnvelopeMigration_2_3 : ISchemaMigration
{
    public string DocumentType => "scenario";
    public int FromVersion => 2;
    public int ToVersion => 3;

    public JsonObject Migrate(JsonObject root)
    {
        root["schemaVersion"] = 3;
        var payload = root["payload"]?.AsObject() ?? root;
        if (payload["continuumDynamic"] is null)
            payload["continuumDynamic"] = new JsonObject
            {
                { "enabled", false },
                { "integrator", "CentralDifference" },
                { "timeStepSeconds", 0.00001 },
                { "durationSeconds", 0.01 },
            };
        return root;
    }
}
