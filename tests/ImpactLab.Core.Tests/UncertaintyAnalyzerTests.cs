using ImpactLab.Core.Experiments.Uncertainty;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class UncertaintyAnalyzerTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal(3, UncertaintyAnalyzer.Summarize("x", [1, 2, 3]).SampleCount);
    }
}
