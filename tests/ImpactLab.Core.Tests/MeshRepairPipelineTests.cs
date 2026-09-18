using ImpactLab.Core.Geometry.Repair;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class MeshRepairPipelineTests
{
    [Fact]
    public void Contract()
    {
        Assert.NotNull(new MeshRepairOptions());
    }
}
