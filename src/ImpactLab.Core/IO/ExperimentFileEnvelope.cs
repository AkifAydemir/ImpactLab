using ImpactLab.Core.Experiments;

namespace ImpactLab.Core.IO;

public sealed class ExperimentFileEnvelope
{
    public string Format { get; set; } = "ImpactLab.Experiment";
    public int SchemaMajor { get; set; } = SchemaVersion.CurrentExperiment.Major;
    public int SchemaMinor { get; set; } = SchemaVersion.CurrentExperiment.Minor;
    public DateTimeOffset SavedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ExperimentDefinition Experiment { get; set; } = new();
}
