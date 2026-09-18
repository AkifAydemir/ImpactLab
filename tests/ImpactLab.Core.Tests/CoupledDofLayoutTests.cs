using ImpactLab.Core.Thermal.Coupled;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CoupledDofLayoutTests
{
    [Fact]
    public void FourDofsPerNode()
    {
        var l = new CoupledDofLayout(5);
        Assert.Equal(20, l.TotalDofs);
        Assert.Equal(7, l.T(1));
    }
}
