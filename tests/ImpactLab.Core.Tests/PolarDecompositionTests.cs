using ImpactLab.Core.Continuum.FiniteStrain;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PolarDecompositionTests
{
    [Fact]
    public void IdentityDecomposes()
    {
        var r = PolarDecomposition.Decompose(DeformationGradient3.Identity);
        Assert.True(r.Residual < 1e-10);
    }
}
