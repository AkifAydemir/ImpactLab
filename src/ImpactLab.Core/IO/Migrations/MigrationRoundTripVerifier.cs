using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public sealed class MigrationRoundTripVerifier
{
    public MigrationRoundTripResult Verify(
        string type,
        int source,
        int target,
        JsonNode fixture,
        DocumentMigrationRegistry registry
    )
    {
        try
        {
            var migrated = registry.Migrate(type, source, target, fixture.DeepClone());
            return new(type, source, target, migrated is not null, null, null);
        }
        catch (Exception ex)
        {
            return new(type, source, target, false, null, ex.Message);
        }
    }
}
