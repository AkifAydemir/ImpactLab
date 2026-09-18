using System.Diagnostics;

namespace ImpactLab.Core.Sparse;

public static class SparseLinearSolverExtensions
{
    public static SparseSolveResult Solve(
        this ISparseLinearSolver solver,
        CsrMatrix matrix,
        ReadOnlySpan<double> rightHandSide,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var solution = new double[matrix.Columns];
        var report = solver.Solve(matrix, rightHandSide, solution, cancellationToken);
        stopwatch.Stop();
        return new(
            solution,
            report.Converged,
            report.Iterations,
            report.ResidualNorm,
            stopwatch.Elapsed
        );
    }
}
