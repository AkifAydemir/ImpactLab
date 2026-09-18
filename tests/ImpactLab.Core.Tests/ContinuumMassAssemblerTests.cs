using ImpactLab.Core.Continuum.Dynamics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ContinuumMassAssemblerTests
{
    [Fact]
    public void Contract()
    {
        Assert.All(
            ContinuumMassAssembler.Lumped(TestMeshes.SingleTetra()),
            x => Assert.True(x > 0)
        );
    }
}
