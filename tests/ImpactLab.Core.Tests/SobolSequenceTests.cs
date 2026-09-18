using ImpactLab.Core.Experiments.Sensitivity;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SobolSequenceTests
{
    [Fact]
    public void Contract()
    {
        var x = SobolSequence.Generate(8, 2);
        Assert.Equal(8, x.GetLength(0));
    }
}
