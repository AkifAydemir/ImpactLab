using ImpactLab.Core.Continuum.Contact;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class AugmentedContactTests
{
    [Fact]
    public void PenetrationCreatesCompression()
    {
        var p = new ContactManifoldPoint(
            new(1, 2, 0),
            Vec3.Zero,
            Vec3.Zero,
            Vec3.UnitZ,
            -1e-4,
            1,
            0,
            0
        );
        var r = new AugmentedLagrangianContactLaw().Evaluate(
            p,
            new(0, 0, 0, true, 0, 0),
            1e8,
            Vec3.Zero,
            1e-3
        );
        Assert.True(r.normal > 0);
    }
}
