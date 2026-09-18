using ImpactLab.Core.Experiments.Optimization;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NelderMeadOptimizerTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal("nelder-mead", new NelderMeadOptimizer().Id);
    }
}
