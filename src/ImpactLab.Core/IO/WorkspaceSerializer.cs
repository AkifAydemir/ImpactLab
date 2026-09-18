using System.Text.Json;

namespace ImpactLab.Core.IO;

public static class WorkspaceSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static void Save(string path, WorkspaceDocument workspace)
    {
        workspace.ModifiedUtc = DateTimeOffset.UtcNow;
        File.WriteAllText(path, JsonSerializer.Serialize(workspace, Options));
    }

    public static WorkspaceDocument Load(string path) =>
        JsonSerializer.Deserialize<WorkspaceDocument>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException("Workspace file is empty.");
}
