namespace ImpactLab.Core.Sparse;

public sealed class NativeSparseLinearSolver : ISparseLinearSolver
{
    public string Id => "native-csr";
    public bool IsAvailable => NativeSparseLibrary.Probe();
    public double Tolerance { get; init; } = 1e-10;
    public int MaxIterations { get; init; } = 20000;

    public LinearSolveReport Solve(
        CsrMatrix a,
        ReadOnlySpan<double> b,
        Span<double> x,
        CancellationToken cancellationToken = default
    )
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Native sparse library is unavailable.");
        var rhs = b.ToArray();
        var sol = x.ToArray();
        var code = NativeSparseLibrary.Solve(
            a.Rows,
            a.Values.Length,
            a.RowOffsets,
            a.ColumnIndices,
            a.Values,
            rhs,
            sol,
            Tolerance,
            MaxIterations,
            out var it,
            out var res
        );
        sol.CopyTo(x);
        return new(code == 0, it, res, Id);
    }
}
