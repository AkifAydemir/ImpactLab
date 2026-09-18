using ImpactLab.Core.Extensions.Security;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExtensionIpcAuthenticatorTests
{
    [Fact]
    public void SignedEnvelopeVerifiesOnce()
    {
        var key = Enumerable.Range(0, 32).Select(x => (byte)x).ToArray();
        var a = new ExtensionIpcAuthenticator(key);
        var p = new IpcAuthorizationPolicy(IpcCapability.ReadScenario, "ext");
        var e = a.Sign(
            "ext",
            "s",
            "c",
            ExtensionIpcMessageKind.Request,
            IpcCapability.ReadScenario,
            "{}"
        );
        var replay = new IpcReplayGuard();
        Assert.True(a.Verify(e, "ext", "s", p, replay, e.TimestampUtc));
        Assert.False(a.Verify(e, "ext", "s", p, replay, e.TimestampUtc));
    }

    [Fact]
    public void TamperedPayloadFails()
    {
        var key = new byte[32];
        var a = new ExtensionIpcAuthenticator(key);
        var p = new IpcAuthorizationPolicy(IpcCapability.ReadResults, "ext");
        var e = a.Sign(
            "ext",
            "s",
            "c",
            ExtensionIpcMessageKind.Request,
            IpcCapability.ReadResults,
            "{\"x\":1}"
        ) with
        {
            PayloadJson = "{\"x\":2}",
        };
        Assert.False(a.Verify(e, "ext", "s", p, new IpcReplayGuard(), e.TimestampUtc));
    }
}
