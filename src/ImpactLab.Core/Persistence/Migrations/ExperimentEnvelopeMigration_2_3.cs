using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class ExperimentEnvelopeMigration_2_3 : ISchemaMigration
{
    public string DocumentType => "experiment";
    public int FromVersion => 2;
    public int ToVersion => 3;

    public JsonObject Migrate(JsonObject root)
    {
        root["schemaVersion"] = 3;
        var p = root["payload"]?.AsObject() ?? root;
        if (p["executionMode"] is null)
            p["executionMode"] = "InProcess";
        if (p["workerQuota"] is null)
            p["workerQuota"] = new JsonObject { { "maxWorkingSetMb", 4096 } };
        return root;
    }
}
