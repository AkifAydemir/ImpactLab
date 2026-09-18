using ImpactLab.Core.Finalization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SourceCoverageCatalogTests
{
    [Fact]
    public void V15CatalogHasUniqueIdsAndPaths()
    {
        var items = SourceCoverageCatalog.CreateV15().Items;
        Assert.NotEmpty(items);
        Assert.Equal(
            items.Count,
            items.Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count()
        );
        Assert.Equal(
            items.Count,
            items.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count()
        );
    }
}
