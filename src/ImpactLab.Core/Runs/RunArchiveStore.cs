using System.Text.Json;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.IO;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Runs;

public sealed class RunArchiveStore
{
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public RunArchiveStore(string rootDirectory)
    {
        RootDirectory = Path.GetFullPath(rootDirectory);
        Directory.CreateDirectory(RootDirectory);
    }

    public string RootDirectory { get; }
    private string IndexPath => Path.Combine(RootDirectory, "index.json");

    public RunArchiveIndex LoadIndex() =>
        File.Exists(IndexPath)
            ? JsonSerializer.Deserialize<RunArchiveIndex>(File.ReadAllText(IndexPath), _json)
                ?? new()
            : new();

    public void SaveIndex(RunArchiveIndex index) =>
        File.WriteAllText(IndexPath, JsonSerializer.Serialize(index, _json));

    public ArchivedRunRecord Save(
        string name,
        ScenarioDefinition scenario,
        SimulationResult result,
        RunResultSummary summary,
        ResultMetricSet metrics,
        IEnumerable<string>? tags = null
    )
    {
        var record = new ArchivedRunRecord
        {
            Name = name,
            ScenarioHash = RunFingerprint.Compute(scenario),
            ComputeTime = result.ComputeTime,
            Metrics = metrics,
        };
        if (tags is not null)
            record.Tags.AddRange(
                tags.Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            );
        var dir = Path.Combine(RootDirectory, record.Id);
        Directory.CreateDirectory(dir);
        File.WriteAllText(
            Path.Combine(dir, record.ScenarioFile),
            JsonSerializer.Serialize(scenario, _json)
        );
        TelemetryCsvWriter.Write(Path.Combine(dir, record.TelemetryFile), result.Telemetry);
        RunSummaryTextWriter.Write(Path.Combine(dir, record.SummaryFile), summary);
        File.WriteAllText(
            Path.Combine(dir, "record.json"),
            JsonSerializer.Serialize(record, _json)
        );
        var index = LoadIndex();
        index.Runs.RemoveAll(x => x.Id == record.Id);
        index.Runs.Add(record);
        SaveIndex(index);
        return record;
    }
}
