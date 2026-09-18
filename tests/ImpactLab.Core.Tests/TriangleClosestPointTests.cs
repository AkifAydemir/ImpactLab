using ImpactLab.Core.Continuum.Contact;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class TriangleClosestPointTests
{
    [Fact]
    public void ProjectsOntoTriangle()
    {
        var q = TriangleClosestPoint.Evaluate(
            new Vec3(.2, .2, 1),
            Vec3.Zero,
            Vec3.UnitX,
            Vec3.UnitY
        );
        Assert.Equal(1, q.Distance, 8);
        Assert.Equal(1, q.U + q.V + q.W, 8);
    }
}
