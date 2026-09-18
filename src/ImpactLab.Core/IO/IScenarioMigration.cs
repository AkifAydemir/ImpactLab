using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO;

public interface IScenarioMigration
{
    int FromMajor { get; }
    int ToMajor { get; }
    JsonObject Apply(JsonObject document);
}
