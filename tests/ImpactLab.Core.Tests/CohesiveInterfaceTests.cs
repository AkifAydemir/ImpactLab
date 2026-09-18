using ImpactLab.Core.Materials;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CohesiveInterfaceTests
{
    [Fact]
    public void DamageGrowsWithSeparation()
    {
        var law = new CohesiveInterfaceLaw("i", FailureSeparationMeters: 1e-3);
        var state = new CohesiveInterfaceState();
        var a = CohesiveInterfaceEvaluator.Evaluate(law, state, 1e-5, 0, 1, 1e-6);
        var b = CohesiveInterfaceEvaluator.Evaluate(law, state, 8e-4, 0, 1, 1e-6);
        Assert.True(b.Damage >= a.Damage);
    }
}
