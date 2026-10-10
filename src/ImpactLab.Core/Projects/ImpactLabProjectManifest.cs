namespace ImpactLab.Core.Projects;

public sealed record ImpactLabProjectManifest(
    int SchemaVersion,
    string Format,
    string ProjectId,
    string Name,
    DateTimeOffset SavedAtUtc,
    string ScenarioEntry,
    IReadOnlyList<string> MaterialEntries,
    IReadOnlyDictionary<string, string> GeometryEntries
)
{
    public const int CurrentSchemaVersion = 2;
    public const string FormatId = "ImpactLab.Project";

    public void Validate()
    {
        if (SchemaVersion is < 1 or > CurrentSchemaVersion || Format != FormatId)
            throw new InvalidDataException(
                $"Unsupported ImpactLab project format {Format} v{SchemaVersion}."
            );
        if (string.IsNullOrWhiteSpace(ProjectId) || string.IsNullOrWhiteSpace(ScenarioEntry))
            throw new InvalidDataException("Project manifest is incomplete.");
    }
}
