using ImpactLab.Core.Continuum.Dynamics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NewmarkBetaIntegratorTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal("newmark-beta", new NewmarkBetaIntegrator().Id);
    }
}
