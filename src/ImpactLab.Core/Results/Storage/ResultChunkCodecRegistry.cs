namespace ImpactLab.Core.Results.Storage;

public sealed class ResultChunkCodecRegistry
{
    private readonly Dictionary<string, IResultChunkCodec> _c = new(
        StringComparer.OrdinalIgnoreCase
    );

    public ResultChunkCodecRegistry()
    {
        Register(new RawResultChunkCodec());
        Register(new DeflateResultChunkCodec());
    }

    public void Register(IResultChunkCodec c) => _c[c.Id] = c;

    public IResultChunkCodec Get(string id) =>
        _c.TryGetValue(id, out var c) ? c : throw new KeyNotFoundException(id);
}
