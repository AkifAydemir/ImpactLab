using ImpactLab.Core.Continuum;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class TetraElementGeometryTests
{
    [Fact]
    public void UnitTetraHasExpectedVolume()
    {
        var v = TetraElementGeometry.SignedVolume(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ);
        Assert.Equal(1.0 / 6.0, v, 10);
    }
}
