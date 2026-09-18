using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public sealed class WorkspaceEnvelopeMigration_1_2 : ISchemaMigration
{
    public string DocumentType => "workspace";
    public int FromVersion => 1;
    public int ToVersion => 2;

    public JsonObject Migrate(JsonObject root)
    {
        root["schemaVersion"] = 2;
        root["activeBackendId"] ??= "explicit-lattice-v1";
        root["selectedWorkspace"] ??= "Simulation";
        return root;
    }
}
