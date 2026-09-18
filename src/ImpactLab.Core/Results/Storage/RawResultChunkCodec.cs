namespace ImpactLab.Core.Results.Storage;

public sealed class RawResultChunkCodec : IResultChunkCodec
{
    public string Id => "raw";

    public byte[] Encode(ReadOnlySpan<byte> d) => d.ToArray();

    public byte[] Decode(ReadOnlySpan<byte> d, int expected) => d.ToArray();
}
