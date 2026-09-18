using ImpactLab.Core.Analysis;

namespace ImpactLab.Core.Runs;

public sealed class ArchivedRunRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Run";
    public string ScenarioHash { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public TimeSpan ComputeTime { get; set; }
    public ResultMetricSet Metrics { get; set; } = new();
    public string ScenarioFile { get; set; } = "scenario.json";
    public string TelemetryFile { get; set; } = "telemetry.csv";
    public string SummaryFile { get; set; } = "summary.txt";
    public List<string> Tags { get; } = [];
    public List<RunAnnotation> Annotations { get; } = [];
}
