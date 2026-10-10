using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class Matrix3ConditioningTests
{
    [Fact]
    public void InverseRejectsSingularAndIllConditionedMatrices()
    {
        var singular = Matrix3.Diagonal(1, 1, 0);
        var illConditioned = Matrix3.Diagonal(1, 1, 1e-14);

        Assert.False(singular.TryInverse(out _, out var singularCondition));
        Assert.True(double.IsPositiveInfinity(singularCondition));
        Assert.Throws<InvalidOperationException>(() => singular.Inverse());
        Assert.False(illConditioned.TryInverse(out _, out _));
        Assert.Throws<InvalidOperationException>(() => illConditioned.Inverse());
    }

    [Fact]
    public void InverseUsesRelativeScaleInsteadOfAbsoluteMagnitude()
    {
        var matrix = Matrix3.Diagonal(1e-9, 2e-9, 3e-9);

        Assert.True(matrix.TryInverse(out var inverse, out var condition));
        var reconstructed = inverse * new Vec3(1e-9, 2e-9, 3e-9);

        Assert.Equal(1, reconstructed.X, 12);
        Assert.Equal(1, reconstructed.Y, 12);
        Assert.Equal(1, reconstructed.Z, 12);
        Assert.Equal(3, condition, 12);
    }
}
