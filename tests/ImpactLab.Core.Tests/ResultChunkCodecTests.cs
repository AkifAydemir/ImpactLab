using ImpactLab.Core.Results.Storage;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ResultChunkCodecTests
{
    [Fact]
    public void Contract()
    {
        var c = new DeflateResultChunkCodec();
        var b = new byte[] { 1, 2, 3, 4 };
        Assert.Equal(b, c.Decode(c.Encode(b), b.Length));
    }
}
