using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO;

public interface IJsonMigrationStep
{
    string Format { get; }
    int FromMajor { get; }
    int ToMajor { get; }
    JsonObject Apply(JsonObject root);
}
