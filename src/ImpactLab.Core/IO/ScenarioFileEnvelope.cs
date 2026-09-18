using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.IO;

public sealed class ScenarioFileEnvelope
{
    public string Format { get; set; } = "ImpactLab.Scenario";
    public int SchemaMajor { get; set; } = SchemaVersion.CurrentScenario.Major;
    public int SchemaMinor { get; set; } = SchemaVersion.CurrentScenario.Minor;
    public DateTimeOffset SavedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ScenarioDefinition Scenario { get; set; } = new();
}
