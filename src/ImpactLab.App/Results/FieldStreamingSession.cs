using ImpactLab.Core.Results.Storage;

namespace ImpactLab.App.Results;

public sealed class FieldStreamingSession : IAsyncDisposable
{
    private readonly IAsyncResultFieldStore _store;
    private CancellationTokenSource? _cts;

    public FieldStreamingSession(IAsyncResultFieldStore store) => _store = store;

    public async ValueTask<double[]> ReadAsync(
        string field,
        int frame,
        CancellationToken ct = default
    ) => await _store.ReadAsync(field, frame, ct);

    public void Cancel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public ValueTask DisposeAsync()
    {
        Cancel();
        return _store.DisposeAsync();
    }
}
