using ImpactLab.Core.Continuum.Contact;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PenaltyFrictionContactLawTests
{
    [Fact]
    public void Contract()
    {
        Assert.True(
            PenaltyFrictionContactLaw
                .Evaluate(
                    ImpactLab.Core.Mathematics.Vec3.UnitZ,
                    .001,
                    new(1, 0, -1),
                    new(default, 0, 0, 0),
                    new(),
                    1e-4,
                    0
                )
                .Force.Z > 0
        );
    }
}
