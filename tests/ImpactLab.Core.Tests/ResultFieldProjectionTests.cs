using ImpactLab.Core.Results.Fields;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ResultFieldProjectionTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(5, ResultFieldProjection.Magnitude(new double[] { 3, 4 }, 2)[0], 6);
    }
}
