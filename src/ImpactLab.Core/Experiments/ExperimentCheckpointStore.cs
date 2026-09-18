using System.Text.Json;

namespace ImpactLab.Core.Experiments;

public sealed class ExperimentCheckpointStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public ExperimentCheckpointStore(string path) => Path = path;

    public string Path { get; }

    public ExperimentCheckpoint? Load() =>
        File.Exists(Path)
            ? JsonSerializer.Deserialize<ExperimentCheckpoint>(File.ReadAllText(Path), Options)
            : null;

    public void Save(ExperimentCheckpoint checkpoint)
    {
        var dir = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
        var temp = Path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(checkpoint, Options));
        File.Move(temp, Path, true);
    }

    public void Delete()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
