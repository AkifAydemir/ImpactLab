using System.IO.Compression;

namespace ImpactLab.Core.Results.Storage;

public sealed class DeflateResultChunkCodec : IResultChunkCodec
{
    public string Id => "deflate";

    public byte[] Encode(ReadOnlySpan<byte> d)
    {
        using var m = new MemoryStream();
        using (var z = new DeflateStream(m, CompressionLevel.Fastest, true))
            z.Write(d);
        return m.ToArray();
    }

    public byte[] Decode(ReadOnlySpan<byte> d, int expected)
    {
        using var i = new MemoryStream(d.ToArray());
        using var z = new DeflateStream(i, CompressionMode.Decompress);
        using var o = new MemoryStream(expected);
        z.CopyTo(o);
        return o.ToArray();
    }
}
