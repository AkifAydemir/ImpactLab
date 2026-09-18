using ImpactLab.Core.Analysis;
using ImpactLab.Core.Runs;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed class RunArchiveService
{
    private readonly RunArchiveStore _store;

    public RunArchiveService(string rootDirectory) => _store = new RunArchiveStore(rootDirectory);

    public IReadOnlyList<ArchivedRunRecord> Query(RunArchiveQuery? query = null) =>
        (query ?? new()).Apply(_store.LoadIndex().Runs).ToArray();

    public ArchivedRunRecord Archive(
        string name,
        ScenarioDefinition scenario,
        SimulationRunBundle bundle,
        IEnumerable<string>? tags = null
    ) =>
        _store.Save(
            name,
            scenario,
            bundle.Result,
            bundle.Summary,
            ResultMetricExtractor.Extract(bundle.Summary),
            tags
        );
}
