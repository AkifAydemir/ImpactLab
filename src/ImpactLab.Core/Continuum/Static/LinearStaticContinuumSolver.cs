using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Static;

public sealed class LinearStaticContinuumSolver
{
    private readonly SparseSolverRegistry _solvers;

    public LinearStaticContinuumSolver(SparseSolverRegistry? solvers = null) =>
        _solvers = solvers ?? SparseSolverRegistry.CreateDefault();

    public ContinuumStaticResult Solve(
        TetrahedralMesh mesh,
        double[] rhs,
        IReadOnlyList<ContinuumDirichletDof> constraints,
        ContinuumStaticSettings settings,
        CancellationToken ct = default,
        Func<TetraElement, double>? stiffnessScale = null
    )
    {
        var k = ContinuumSystemAssembler.AssembleStiffness(mesh, stiffnessScale);
        var reduced = ContinuumBoundaryAssembler.Apply(k, rhs, constraints);
        var u = new double[rhs.Length];
        var solver = settings.PreferNative
            ? _solvers.CreatePreferred(true)
            : _solvers.Create(settings.SparseSolverId);
        var report = solver.Solve(reduced.Matrix, reduced.Rhs, u, ct);
        var disp = new Vec3[mesh.Nodes.Count];
        for (var i = 0; i < disp.Length; i++)
            disp[i] = new(u[i * 3], u[i * 3 + 1], u[i * 3 + 2]);
        var recovered = ContinuumStressRecovery.Recover(mesh, u);
        return new(disp, recovered.Stress, recovered.Strain, report, 1.0);
    }
}
