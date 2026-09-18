using ImpactLab.Core.Verification;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SignalComparatorTests
{
    [Fact]
    public void IdenticalSignalsHaveZeroError()
    {
        var r = new ReferenceSignal("r", "r", "", [new(0, 0), new(1, 2), new(2, 4)]);
        var m = SignalComparator.Compare(r, r.Points);
        Assert.Equal(0, m.Rmse, 10);
        Assert.Equal(1, m.Correlation, 10);
    }
}
