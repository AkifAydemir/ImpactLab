using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO;

public sealed class ScenarioMigrationV1ToV2 : IJsonMigrationStep
{
    public string Format => "ImpactLab.Scenario";
    public int FromMajor => 1;
    public int ToMajor => 2;

    public JsonObject Apply(JsonObject root)
    {
        var scenario =
            root["Scenario"] as JsonObject
            ?? root["scenario"] as JsonObject
            ?? throw new InvalidDataException("Scenario payload missing.");
        if (scenario["Meshing"] is null)
        {
            var cell = scenario["CellSizeMeters"]?.GetValue<double>() ?? 0.01;
            scenario["Meshing"] = new JsonObject
            {
                { "Mode", "UniformStructured" },
                { "BaseCellSizeMeters", cell },
                { "MinimumCellSizeMeters", cell / 4.0 },
                { "MaximumRefinementDepth", 3 },
                { "BoundaryRefinementBandMeters", cell * 1.5 },
            };
        }
        root["SchemaMinor"] = 0;
        return root;
    }
}
