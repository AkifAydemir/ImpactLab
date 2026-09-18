namespace ImpactLab.Core.Sparse.Native;

public interface INativeSparseSolverBridge
{
    NativeSolveResponse Solve(NativeSolveRequest request, CancellationToken ct = default);
}
