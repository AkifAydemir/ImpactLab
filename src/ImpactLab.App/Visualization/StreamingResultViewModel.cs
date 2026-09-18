using ImpactLab.Core.Results.Storage;

namespace ImpactLab.App.Visualization;

public sealed class StreamingResultViewModel
{
    private readonly IResultFieldStore _store;
    private readonly ResultFrameCache _cache = new(16);

    public StreamingResultViewModel(IResultFieldStore store) => _store = store;

    public double[] Load(string field, int frame) =>
        _cache.GetOrAdd(field, frame, () => _store.ReadFrame(field, frame));

    public IReadOnlyList<int> Frames(string field) => _store.Frames(field);
}
