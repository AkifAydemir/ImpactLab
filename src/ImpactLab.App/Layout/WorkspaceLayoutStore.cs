using System.Text.Json;

namespace ImpactLab.App.Layout;

public sealed class WorkspaceLayoutStore
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public void Save(string path, WorkspaceLayoutState state)
    {
        var errors = WorkspaceLayoutValidator.Validate(state);
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", errors));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var envelope = new WorkspaceLayoutEnvelope(
            LayoutSchema.CurrentVersion,
            state,
            DateTimeOffset.UtcNow
        );
        File.WriteAllText(path, JsonSerializer.Serialize(envelope, Json));
    }

    public WorkspaceLayoutState Load(string path) => LoadSafe(path).Layout;

    public WorkspaceLayoutRecoveryResult LoadSafe(
        string path,
        WorkspaceLayoutState? fallback = null
    )
    {
        fallback ??= Default();
        try
        {
            var text = File.ReadAllText(path);
            WorkspaceLayoutEnvelope? e = null;
            try
            {
                e = JsonSerializer.Deserialize<WorkspaceLayoutEnvelope>(text, Json);
            }
            catch (JsonException) { }
            if (e is null)
            {
                var legacy =
                    JsonSerializer.Deserialize<WorkspaceLayoutState>(text, Json)
                    ?? throw new InvalidDataException("Invalid layout file.");
                e = new(1, legacy, DateTimeOffset.MinValue);
            }
            e = WorkspaceLayoutMigration.Migrate(e);
            var errors = WorkspaceLayoutValidator.Validate(e.Layout);
            if (errors.Count > 0)
                return new(fallback, true, errors);
            return new(e.Layout, e.SchemaVersion != LayoutSchema.CurrentVersion, []);
        }
        catch (Exception ex)
        {
            return new(fallback, true, [ex.Message]);
        }
    }

    public static WorkspaceLayoutState Default() =>
        new(
            "Default",
            [
                new("navigation", true, 220, 800, "Left", 0),
                new("properties", true, 320, 800, "Right", 1),
                new("timeline", true, 120, 300, "Bottom", 2),
            ],
            "Simulation"
        );
}
