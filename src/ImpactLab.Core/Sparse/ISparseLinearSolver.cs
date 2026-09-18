namespace ImpactLab.Core.Sparse;

public interface ISparseLinearSolver
{
    string Id { get; }
    bool IsAvailable { get; }
    LinearSolveReport Solve(
        CsrMatrix matrix,
        ReadOnlySpan<double> rhs,
        Span<double> solution,
        CancellationToken cancellationToken = default
    );
}
