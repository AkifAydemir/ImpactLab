using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Materials;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class NeoHookeanTests
{
    [Fact]
    public void IdentityHasFiniteStress()
    {
        var m = MaterialLibrary.All[0];
        var k = new FiniteStrainKinematics(
            DeformationGradient3.Identity,
            new Matrix3(),
            new Matrix3(),
            new Matrix3(),
            new Matrix3(),
            1
        );
        var r = new NeoHookeanLaw().Update(
            new(m, FiniteStrainElementState.Initial(), k, 1e-3, 293.15)
        );
        Assert.True(double.IsFinite(r.KirchhoffStress.Trace));
    }
}
