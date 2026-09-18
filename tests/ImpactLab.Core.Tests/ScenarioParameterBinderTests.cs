using ImpactLab.Core.Experiments;
using ImpactLab.Core.Scenarios;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ScenarioParameterBinderTests
{
    [Fact]
    public void AppliesScenarioCellSize()
    {
        var s = new ScenarioDefinition();
        ScenarioParameterBinder.Apply(s, [new("scenario:cellSizeMeters", .02)]);
        Assert.Equal(.02, s.CellSizeMeters, 8);
    }
}
