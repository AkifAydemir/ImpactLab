using ImpactLab.Core.Experiments.Workers;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class WorkerCoordinatorTests
{
    [Fact]
    public void SelectsCompatibleNode()
    {
        var c = new WorkerCoordinator();
        c.Register(new WorkerHello("w", new("1", "8", "os", 4, 1_000_000, ["b"]), 1));
        Assert.Equal("w", c.Select(["b"])!.WorkerId);
    }
}
