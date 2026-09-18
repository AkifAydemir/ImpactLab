using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO.Migrations;

public interface IDocumentMigration
{
    string DocumentType { get; }
    int FromVersion { get; }
    int ToVersion { get; }
    JsonNode Migrate(JsonNode root);
}
