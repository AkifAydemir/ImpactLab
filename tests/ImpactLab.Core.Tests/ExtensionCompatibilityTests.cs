using ImpactLab.Core.Extensions;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExtensionCompatibilityTests
{
    [Fact]
    public void RejectsFutureHostRequirement()
    {
        var m = new ExtensionPackageManifest("x", "X", "1", "x.dll", null, "99.0.0");
        Assert.False(ExtensionCompatibility.Check(m, new Version(9, 0, 0)).Compatible);
    }
}
