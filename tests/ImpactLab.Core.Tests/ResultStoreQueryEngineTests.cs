using ImpactLab.Core.Results.Storage;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ResultStoreQueryEngineTests
{
    [Fact]
    public void FiltersField()
    {
        var now = DateTimeOffset.UtcNow;
        var s = new ResultStoreCatalogSnapshot(
            1,
            now,
            [
                new("a", "s", ["stress"], 2, 0, 1, 10, now),
                new("b", "s", ["temperature"], 2, 0, 1, 20, now),
            ]
        );
        var r = ResultStoreQueryEngine.Query(s, new(RequiredField: "stress"));
        Assert.Single(r);
        Assert.Equal("a", r[0].RunId);
    }
}
