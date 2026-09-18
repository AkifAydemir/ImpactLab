using ImpactLab.Core.Geometry.Cad;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CadTopologyInspectorTests
{
    [Fact]
    public void EmptyModelHasNoIssues()
    {
        var m = new CadModel([], [], [], [], "test", null);
        Assert.Empty(CadTopologyInspector.Inspect(m));
    }
}
