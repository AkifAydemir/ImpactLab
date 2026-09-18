using ImpactLab.Core.Continuum.FiniteStrain;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FiniteStrainMatrixTests
{
    [Fact]
    public void IdentityDeterminantIsOne() => Assert.Equal(1, Matrix3.Identity.Determinant, 12);
}
