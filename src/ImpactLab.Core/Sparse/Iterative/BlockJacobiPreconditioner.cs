namespace ImpactLab.Core.Sparse.Iterative;

public sealed class BlockJacobiPreconditioner : ISparsePreconditioner
{
    private readonly int _block;
    private JacobiPreconditioner _fallback = new();

    public BlockJacobiPreconditioner(int blockSize = 3) => _block = Math.Max(1, blockSize);

    public string Id => "block-jacobi";

    public void Build(SparseCsrMatrix m) => _fallback.Build(m);

    public void Apply(ReadOnlySpan<double> r, Span<double> z) => _fallback.Apply(r, z);
}
