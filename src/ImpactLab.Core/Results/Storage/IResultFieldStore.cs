using ImpactLab.Core.Results.Fields;

namespace ImpactLab.Core.Results.Storage;

public interface IResultFieldStore : IDisposable
{
    string RunId { get; }
    ResultFieldRegistry Registry { get; }
    void WriteFrame(string fieldId, int frameIndex, ReadOnlySpan<double> values, int components);
    double[] ReadFrame(string fieldId, int frameIndex);
    IReadOnlyList<int> Frames(string fieldId);
    void Flush();
}
