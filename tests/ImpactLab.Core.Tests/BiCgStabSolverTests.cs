using ImpactLab.Core.Sparse;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class BiCgStabSolverTests
{
    [Fact]
    public void Contract()
    {
        Assert.True(new BiCgStabSolver().IsAvailable);
    }
}
