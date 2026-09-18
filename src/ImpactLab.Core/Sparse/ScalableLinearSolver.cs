using ImpactLab.Core.Sparse.Iterative;

namespace ImpactLab.Core.Sparse;

public sealed class ScalableLinearSolver
{
    private readonly PreconditionerRegistry _p = PreconditionerRegistry.CreateDefault();

    public SparseSolveResult Solve(
        SparseCsrMatrix A,
        double[] b,
        ScalableLinearSolverSettings? settings = null,
        CancellationToken ct = default
    )
    {
        var s = settings ?? new();
        var pre = _p.Create(s.PreconditionerId);
        var (x, m) = s.SolverId.Equals("pcg", StringComparison.OrdinalIgnoreCase)
            ? new PcgSolver().Solve(A, b, pre, new(MaxIterations: 5000), ct)
            : new GmresSolver().Solve(A, b, pre, new(MaxIterations: 5000), ct);
        return new(x, m.Converged, m.Iterations, m.FinalResidual, m.Elapsed);
    }
}
