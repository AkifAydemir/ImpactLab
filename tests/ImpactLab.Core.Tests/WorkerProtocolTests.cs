using ImpactLab.Core.Experiments.Workers;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class WorkerProtocolTests
{
    [Fact]
    public void EnvelopeRoundTrips()
    {
        var h = new WorkerHeartbeat("w", "c", TimeSpan.FromSeconds(1), 123, 4);
        var text = WorkerJsonProtocol.Envelope(WorkerMessageKind.Heartbeat, "r", h);
        var e = WorkerJsonProtocol.Deserialize<WorkerProtocolEnvelope>(text);
        Assert.Equal("c", WorkerJsonProtocol.Payload<WorkerHeartbeat>(e).CaseId);
    }
}
