namespace ImpactLab.Core.Results.Storage;

public sealed class ResultFrameCache
{
    private readonly int _capacity;
    private readonly Dictionary<(string, int), double[]> _cache = [];
    private readonly LinkedList<(string, int)> _lru = [];

    public ResultFrameCache(int capacity = 12) => _capacity = Math.Max(1, capacity);

    public double[] GetOrAdd(string field, int frame, Func<double[]> factory)
    {
        var k = (field, frame);
        if (_cache.TryGetValue(k, out var v))
        {
            Touch(k);
            return v;
        }
        v = factory();
        _cache[k] = v;
        _lru.AddFirst(k);
        while (_cache.Count > _capacity)
        {
            var last = _lru.Last!.Value;
            _lru.RemoveLast();
            _cache.Remove(last);
        }
        return v;
    }

    private void Touch((string, int) k)
    {
        var n = _lru.Find(k);
        if (n is null)
            return;
        _lru.Remove(n);
        _lru.AddFirst(n);
    }
}
