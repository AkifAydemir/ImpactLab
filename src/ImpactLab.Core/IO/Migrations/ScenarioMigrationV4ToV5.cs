using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class ScenarioMigrationV4ToV5 : IDocumentMigration
{
    public string DocumentType => "scenario";
    public int FromVersion => 4;
    public int ToVersion => 5;

    public JsonNode Migrate(JsonNode root)
    {
        var p = root["payload"]!.AsObject();
        var c = (p["continuum"] ??= new JsonObject()).AsObject();
        c["advanced"] ??= new JsonObject
        {
            { "enableFiniteStrain", false },
            { "finiteStrainLawId", "finite-j2" },
            { "contactMethod", "AugmentedLagrangian" },
            {
                "monolithicCoupling",
                new JsonObject { { "maxNewtonIterations", 25 } }
            },
        };
        root["schemaVersion"] = 5;
        return root;
    }
}
