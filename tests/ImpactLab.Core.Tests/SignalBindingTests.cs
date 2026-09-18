using ImpactLab.Core.Verification;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SignalBindingTests
{
    [Fact]
    public void ProbeBindingRequiresSource()
    {
        var b = new SignalBinding("ref", SignalBindingKind.ProbeAverage);
        Assert.Throws<InvalidOperationException>(() => b.Validate());
    }
}
