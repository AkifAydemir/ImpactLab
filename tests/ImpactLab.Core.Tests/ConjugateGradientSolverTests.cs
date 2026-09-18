using ImpactLab.Core.Sparse;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ConjugateGradientSolverTests
{
    [Fact]
    public void SolvesSmallSpdSystem()
    {
        var t = new SparseTripletBuilder();
        t.Add(0, 0, 4);
        t.Add(0, 1, 1);
        t.Add(1, 0, 1);
        t.Add(1, 1, 3);
        var a = t.Build(2, 2);
        var x = new double[2];
        var r = new ConjugateGradientSolver().Solve(a, new double[] { 1, 2 }, x);
        Assert.True(r.Converged);
        Assert.InRange(x[0], .08, .1);
        Assert.InRange(x[1], .63, .65);
    }
}
