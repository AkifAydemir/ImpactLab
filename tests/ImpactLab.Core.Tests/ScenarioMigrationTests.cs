using System.Text.Json.Nodes;
using ImpactLab.Core.IO;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ScenarioMigrationTests
{
    [Fact]
    public void AddsMeshingBlock()
    {
        var root = JsonNode
            .Parse(
                """{"Format":"ImpactLab.Scenario","SchemaMajor":1,"Scenario":{"CellSizeMeters":0.02}}"""
            )!
            .AsObject();
        var migrated = new ScenarioMigrationV1ToV2().Apply(root);
        Assert.NotNull(migrated["Scenario"]!["Meshing"]);
    }
}
