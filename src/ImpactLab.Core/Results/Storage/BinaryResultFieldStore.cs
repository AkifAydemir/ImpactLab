using System.Buffers.Binary;
using System.Text.Json;
using ImpactLab.Core.Results.Fields;

namespace ImpactLab.Core.Results.Storage;

public sealed class BinaryResultFieldStore : IResultFieldStore
{
    private readonly string _root;
    private readonly ResultChunkCodecRegistry _codecs = new();
    private readonly Dictionary<string, SortedSet<int>> _frames = new(
        StringComparer.OrdinalIgnoreCase
    );

    public BinaryResultFieldStore(string root, string runId, ResultFieldRegistry registry)
    {
        _root = root;
        RunId = runId;
        Registry = registry;
        Directory.CreateDirectory(Path.Combine(root, runId));
    }

    public string RunId { get; }
    public ResultFieldRegistry Registry { get; }

    public void WriteFrame(
        string fieldId,
        int frameIndex,
        ReadOnlySpan<double> values,
        int components
    )
    {
        Registry.Get(fieldId);
        var raw = new byte[values.Length * 8];
        Buffer.BlockCopy(values.ToArray(), 0, raw, 0, raw.Length);
        var codec = _codecs.Get("deflate");
        var enc = codec.Encode(raw);
        var p = PathFor(fieldId, frameIndex);
        File.WriteAllBytes(p, enc);
        Meta(
            fieldId,
            frameIndex,
            new()
            {
                ["count"] = values.Length,
                ["components"] = components,
                ["codec"] = codec.Id,
                ["rawBytes"] = raw.Length,
            }
        );
        if (!_frames.TryGetValue(fieldId, out var s))
            _frames[fieldId] = s = [];
        s.Add(frameIndex);
    }

    public double[] ReadFrame(string fieldId, int frameIndex)
    {
        var meta = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            File.ReadAllText(MetaPath(fieldId, frameIndex))
        )!;
        var codec = _codecs.Get(meta["codec"].GetString()!);
        var raw = codec.Decode(
            File.ReadAllBytes(PathFor(fieldId, frameIndex)),
            meta["rawBytes"].GetInt32()
        );
        var a = new double[raw.Length / 8];
        Buffer.BlockCopy(raw, 0, a, 0, raw.Length);
        return a;
    }

    public IReadOnlyList<int> Frames(string fieldId) =>
        _frames.TryGetValue(fieldId, out var s) ? s.ToArray() : [];

    public void Flush() { }

    public void Dispose() { }

    private string PathFor(string f, int i)
    {
        var d = Path.Combine(_root, RunId, Safe(f));
        Directory.CreateDirectory(d);
        return Path.Combine(d, $"{i:D8}.bin");
    }

    private string MetaPath(string f, int i) => Path.ChangeExtension(PathFor(f, i), ".json");

    private void Meta(string f, int i, Dictionary<string, object> m) =>
        File.WriteAllText(MetaPath(f, i), JsonSerializer.Serialize(m));

    private static string Safe(string s) =>
        string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
}
