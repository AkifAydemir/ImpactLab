using ImpactLab.Core.Backends;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class BackendRegistryTests
{
    [Fact]
    public void DefaultRegistryContainsLatticeBackend()
    {
        var r = SimulationBackendRegistry.CreateDefault();
        Assert.Contains(r.Describe(), x => x.Id == ExplicitLatticeBackend.BackendId);
    }
}
