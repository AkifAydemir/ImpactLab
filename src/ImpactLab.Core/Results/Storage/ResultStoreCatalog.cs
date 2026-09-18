namespace ImpactLab.Core.Results.Storage;

public sealed class ResultStoreCatalog
{
    private readonly ResultStoreCatalogIndexer _indexer;

    public ResultStoreCatalog(string root) => _indexer = new(root);

    public IReadOnlyList<ResultStoreRunIndex> List() => _indexer.LoadOrRebuild().Runs;

    public IReadOnlyList<ResultStoreRunIndex> Query(ResultStoreQuery query) =>
        ResultStoreQueryEngine.Query(_indexer.LoadOrRebuild(), query);

    public ResultStoreCatalogSnapshot RebuildIndex() => _indexer.Rebuild();
}
