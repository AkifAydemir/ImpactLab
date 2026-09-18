using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class J2ReturnMapperTests
{
    [Fact]
    public void Contract()
    {
        Assert.True(
            new J2ReturnMapper()
                .Update(
                    TestMaterials.Steel,
                    new StrainTensor6(.02, 0, 0, 0, 0, 0),
                    PlasticState.Zero,
                    new LinearIsotropicHardening(250e6, 1e9)
                )
                .Yielded
        );
    }
}
