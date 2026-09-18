namespace ImpactLab.Core.Results.Storage;

public interface IResultChunkCodec
{
    string Id { get; }
    byte[] Encode(ReadOnlySpan<byte> data);
    byte[] Decode(ReadOnlySpan<byte> data, int expectedLength);
}
