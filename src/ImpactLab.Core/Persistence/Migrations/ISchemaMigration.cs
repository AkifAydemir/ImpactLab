using System.Text.Json.Nodes;

namespace ImpactLab.Core.Persistence.Migrations;

public interface ISchemaMigration
{
    string DocumentType { get; }
    int FromVersion { get; }
    int ToVersion { get; }
    JsonObject Migrate(JsonObject root);
}
