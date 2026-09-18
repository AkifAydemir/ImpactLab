using ImpactLab.Core.Continuum.Nonlinear;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NonlinearConvergenceTests
{
    [Fact]
    public void Contract()
    {
        var s = new NonlinearConvergenceSettings();
        s.Validate();
        Assert.True(s.MaxIterations > 0);
    }
}
