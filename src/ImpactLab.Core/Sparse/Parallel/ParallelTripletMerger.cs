using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Sparse.Parallel;

public static class ParallelTripletMerger
{
    public static SparseCsrMatrix Merge(
        int rows,
        int cols,
        IEnumerable<ParallelAssemblyBuffer> buffers
    )
    {
        var t = new SparseTripletBuilder(rows, cols);
        foreach (var b in buffers)
        foreach (var x in b.Triplets)
            t.Add(x.Row, x.Col, x.Value);
        return t.BuildCsr();
    }
}
