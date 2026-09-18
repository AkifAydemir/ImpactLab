namespace ImpactLab.Core.Results.Storage;

public interface IAsyncResultFieldStore : IAsyncDisposable
{
    ValueTask WriteAsync(AsyncResultWriteRequest request, CancellationToken ct = default);
    ValueTask<double[]> ReadAsync(string fieldId, int frameIndex, CancellationToken ct = default);
    IAsyncEnumerable<int> FramesAsync(string fieldId, CancellationToken ct = default);
}
