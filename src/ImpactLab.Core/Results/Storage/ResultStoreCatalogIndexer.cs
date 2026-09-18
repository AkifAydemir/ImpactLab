using System.Text.Json;

namespace ImpactLab.Core.Results.Storage;

public sealed class ResultStoreCatalogIndexer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _root;
    private readonly string _indexPath;

    public ResultStoreCatalogIndexer(string root)
    {
        _root = Path.GetFullPath(root);
        _indexPath = Path.Combine(_root, "result-catalog.json");
    }

    public ResultStoreCatalogSnapshot Rebuild()
    {
        Directory.CreateDirectory(_root);
        var runs = Directory
            .EnumerateFiles(_root, "run-index.json", SearchOption.AllDirectories)
            .Select(Read)
            .Where(x => x is not null)
            .Cast<ResultStoreRunIndex>()
            .GroupBy(x => x.RunId, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.OrderByDescending(r => r.CreatedUtc).First())
            .OrderByDescending(x => x.CreatedUtc)
            .ToArray();
        var snapshot = new ResultStoreCatalogSnapshot(
            ResultStoreCatalogSnapshot.CurrentSchema,
            DateTimeOffset.UtcNow,
            runs
        );
        File.WriteAllText(_indexPath, JsonSerializer.Serialize(snapshot, Options));
        return snapshot;
    }

    public ResultStoreCatalogSnapshot LoadOrRebuild()
    {
        if (!File.Exists(_indexPath))
            return Rebuild();
        try
        {
            return JsonSerializer.Deserialize<ResultStoreCatalogSnapshot>(
                    File.ReadAllText(_indexPath)
                ) ?? Rebuild();
        }
        catch (JsonException)
        {
            return Rebuild();
        }
    }

    private static ResultStoreRunIndex? Read(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<ResultStoreRunIndex>(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
