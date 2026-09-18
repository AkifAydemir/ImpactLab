using ImpactLab.Core.Results.Fields;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ResultFieldRegistryTests
{
    [Fact]
    public void Contract()
    {
        var r = ResultFieldRegistry.CreateDefault();
        Assert.True(r.Contains("stress"));
    }
}
