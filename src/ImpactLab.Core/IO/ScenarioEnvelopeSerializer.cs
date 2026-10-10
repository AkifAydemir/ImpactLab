using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.IO;

public static class ScenarioEnvelopeSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Save(string path, ScenarioDefinition scenario) =>
        File.WriteAllText(path, Serialize(scenario));

    public static string Serialize(ScenarioDefinition scenario) =>
        JsonSerializer.Serialize(new ScenarioFileEnvelope { Scenario = scenario }, Options);

    public static ScenarioDefinition Load(string path)
    {
        return Deserialize(File.ReadAllText(path));
    }

    public static ScenarioDefinition Deserialize(string text)
    {
        var root =
            JsonNode.Parse(text)?.AsObject()
            ?? throw new InvalidDataException("Scenario file is empty.");
        var format = root["Format"]?.GetValue<string>() ?? root["format"]?.GetValue<string>() ?? "";
        if (format != "ImpactLab.Scenario")
            throw new InvalidDataException($"Unexpected scenario format: {format}");
        var major =
            root["SchemaMajor"]?.GetValue<int>() ?? root["schemaMajor"]?.GetValue<int>() ?? 1;
        if (major > SchemaVersion.CurrentScenario.Major)
            throw new NotSupportedException($"Scenario schema {major} is newer than this build.");
        if (major < SchemaVersion.CurrentScenario.Major)
        {
            var pipeline = new JsonMigrationPipeline();
            pipeline.Register(new ScenarioMigrationV1ToV2());
            root = pipeline.Migrate(
                root,
                "ImpactLab.Scenario",
                SchemaVersion.CurrentScenario.Major
            );
            text = root.ToJsonString(Options);
        }
        var env =
            JsonSerializer.Deserialize<ScenarioFileEnvelope>(text, Options)
            ?? throw new InvalidDataException("Scenario envelope is invalid.");
        env.Scenario.Validate();
        return env.Scenario;
    }
}
