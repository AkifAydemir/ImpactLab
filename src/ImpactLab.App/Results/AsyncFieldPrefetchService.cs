using ImpactLab.Core.Results.Storage;

namespace ImpactLab.App.Results;

public sealed class AsyncFieldPrefetchService : IDisposable
{
    private readonly IAsyncResultFieldStore _store;
    private readonly SemaphoreSlim _slots;
    private readonly Dictionary<(string, int), double[]> _cache = [];
    private readonly object _gate = new();

    public AsyncFieldPrefetchService(IAsyncResultFieldStore store, int maxConcurrency = 2)
    {
        _store = store;
        _slots = new(Math.Max(1, maxConcurrency));
    }

    public bool TryGet(string field, int frame, out double[] values)
    {
        lock (_gate)
            return _cache.TryGetValue((field, frame), out values!);
    }

    public async Task PrefetchAsync(ResultPrefetchPlan plan, CancellationToken ct = default)
    {
        await Task.WhenAll(plan.Items.Select(x => One(x, ct)));
    }

    private async Task One(ResultPrefetchItem item, CancellationToken ct)
    {
        lock (_gate)
            if (_cache.ContainsKey((item.FieldId, item.FrameIndex)))
                return;
        await _slots.WaitAsync(ct);
        try
        {
            var data = await _store.ReadAsync(item.FieldId, item.FrameIndex, ct);
            lock (_gate)
                _cache[(item.FieldId, item.FrameIndex)] = data;
        }
        finally
        {
            _slots.Release();
        }
    }

    public void Trim(int maxEntries = 24)
    {
        lock (_gate)
            foreach (var key in _cache.Keys.Take(Math.Max(0, _cache.Count - maxEntries)).ToArray())
                _cache.Remove(key);
    }

    public void Dispose() => _slots.Dispose();
}
