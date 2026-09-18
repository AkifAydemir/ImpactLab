namespace ImpactLab.Core.Reporting;

public sealed class ReportAssetStore
{
    private readonly Dictionary<string, byte[]> _a = new(StringComparer.OrdinalIgnoreCase);

    public void Put(string name, byte[] data) => _a[name] = data;

    public bool TryGet(string name, out byte[] data) => _a.TryGetValue(name, out data!);

    public IReadOnlyList<string> Names => _a.Keys.OrderBy(x => x).ToArray();
}
