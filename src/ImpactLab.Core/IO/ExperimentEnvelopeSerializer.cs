using System.Text.Json;
using System.Text.Json.Serialization;
using ImpactLab.Core.Experiments;

namespace ImpactLab.Core.IO;

public static class ExperimentEnvelopeSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Save(string path, ExperimentDefinition definition) =>
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(
                new ExperimentFileEnvelope { Experiment = definition },
                Options
            )
        );

    public static ExperimentDefinition Load(string path)
    {
        var e =
            JsonSerializer.Deserialize<ExperimentFileEnvelope>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Experiment file empty.");
        if (e.Format != "ImpactLab.Experiment")
            throw new InvalidDataException("Unexpected experiment format.");
        if (e.SchemaMajor > SchemaVersion.CurrentExperiment.Major)
            throw new NotSupportedException("Experiment schema is newer than this build.");
        return e.Experiment;
    }
}
