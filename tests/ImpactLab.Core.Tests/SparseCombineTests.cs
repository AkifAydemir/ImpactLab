using ImpactLab.Core.Sparse;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class SparseCombineTests
{
    [Fact]
    public void Contract()
    {
        var b = new SparseTripletBuilder(1, 1);
        b.Add(0, 0, 2);
        var m = b.BuildCsr();
        Assert.Equal(6, SparseMatrixAlgebra.Combine((m, 3)).Values[0], 6);
    }
}
