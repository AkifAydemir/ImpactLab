using ImpactLab.Core.Extensions.Security;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExtensionTrustPolicyTests
{
    [Fact]
    public void Contract()
    {
        var p = new ExtensionTrustPolicy();
        var m = new ExtensionManifest(
            "x",
            "X",
            "1",
            "a",
            "t",
            "1.0",
            ExtensionPermission.ReadProject,
            null,
            null,
            []
        );
        p.Validate(m, new(ExtensionTrustLevel.Untrusted, false, "ok"));
    }
}
