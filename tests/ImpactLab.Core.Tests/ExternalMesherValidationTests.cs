using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Meshing.External;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExternalMesherValidationTests
{
    [Fact]
    public void AcceptsValidSingleTetra()
    {
        var m = MaterialLibrary.Get("generic-steel");
        var mesh = new TetrahedralMesh(
            [
                new(0, new Vec3(0, 0, 0), "p", m),
                new(1, new Vec3(1, 0, 0), "p", m),
                new(2, new Vec3(0, 1, 0), "p", m),
                new(3, new Vec3(0, 0, 1), "p", m),
            ],
            [new(0, 0, 1, 2, 3, "p", m)]
        );
        Assert.True(ExternalMesherResultValidator.Validate(mesh).Passed);
    }
}
