using System.Text.Json.Nodes;
using ImpactLab.Core.IO.Migrations;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class DocumentMigrationRegistryTests
{
    [Fact]
    public void ScenarioChainAdvancesFromV3ToV5()
    {
        var root = JsonNode.Parse("{\"schemaVersion\":3,\"payload\":{}}")!;
        var migrated = DocumentMigrationRegistry.CreateDefault().Migrate("scenario", 3, 5, root);
        Assert.Equal(5, migrated["schemaVersion"]!.GetValue<int>());
        Assert.NotNull(migrated["payload"]!["continuum"]);
    }
}
