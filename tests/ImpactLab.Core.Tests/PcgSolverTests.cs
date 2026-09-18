using ImpactLab.Core.Sparse;
using ImpactLab.Core.Sparse.Iterative;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PcgSolverTests
{
    [Fact]
    public void SolvesDiagonal()
    {
        var t = new SparseTripletBuilder(2, 2);
        t.Add(0, 0, 2);
        t.Add(1, 1, 4);
        var A = t.BuildCsr();
        var (x, m) = new PcgSolver().Solve(
            A,
            [2.0, 8.0],
            new ImpactLab.Core.Sparse.Iterative.JacobiPreconditioner()
        );
        Assert.True(m.Converged);
        Assert.Equal(1, x[0], 8);
        Assert.Equal(2, x[1], 8);
    }
}
