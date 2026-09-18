using ImpactLab.Core.Experiments.Optimization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ParetoFrontTests
{
    [Fact]
    public void RemovesDominated()
    {
        var p = new[]
        {
            new MultiObjectiveValue([1.0, 1.0], [], "a"),
            new MultiObjectiveValue([2.0, 2.0], [], "b"),
        };
        Assert.Single(ParetoFrontBuilder.Build(p));
    }
}
