using System.Text.Json;
using System.Text.Json.Serialization;
using ImpactLab.Core.Experiments;

namespace ImpactLab.Core.IO;

public static class ExperimentJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Save(string path, ExperimentDefinition definition) =>
        File.WriteAllText(path, JsonSerializer.Serialize(definition, Options));

    public static ExperimentDefinition Load(string path) =>
        JsonSerializer.Deserialize<ExperimentDefinition>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException("Experiment JSON is empty or invalid.");
}
