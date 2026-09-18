using ImpactLab.Core.Continuum.Dynamics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CentralDifferenceIntegratorTests
{
    [Fact]
    public void Contract()
    {
        Assert.Equal("central-difference", new CentralDifferenceIntegrator().Id);
    }
}
