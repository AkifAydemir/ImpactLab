namespace ImpactLab.Core.Sparse.Iterative;

public interface ISparsePreconditioner
{
    string Id { get; }
    void Build(SparseCsrMatrix matrix);
    void Apply(ReadOnlySpan<double> r, Span<double> z);
}
