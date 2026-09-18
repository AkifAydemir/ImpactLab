using ImpactLab.Core.Continuum;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal;

public sealed class ImplicitThermalSolver
{
    private readonly ISparseLinearSolver _solver;

    public ImplicitThermalSolver(ISparseLinearSolver? solver = null) =>
        _solver = solver ?? new ConjugateGradientSolver(1e-10, 20000);

    public LinearSolveReport Step(
        TetrahedralMesh mesh,
        ThermalState state,
        ThermalPropertyTable props,
        IReadOnlyList<ThermalBoundaryCondition> boundaries,
        IReadOnlyList<ThermalSource> sources,
        double dt,
        CancellationToken ct = default
    )
    {
        var sys = ThermalSystemAssembler.Assemble(mesh, props);
        var t = new SparseTripletBuilder();
        for (var r = 0; r < mesh.Nodes.Count; r++)
        {
            for (var q = sys.Capacity.RowOffsets[r]; q < sys.Capacity.RowOffsets[r + 1]; q++)
                t.Add(r, sys.Capacity.ColumnIndices[q], sys.Capacity.Values[q] / dt);
            for (
                var q = sys.Conductivity.RowOffsets[r];
                q < sys.Conductivity.RowOffsets[r + 1];
                q++
            )
                t.Add(r, sys.Conductivity.ColumnIndices[q], sys.Conductivity.Values[q]);
        }
        var a = t.Build(mesh.Nodes.Count, mesh.Nodes.Count);
        var rhs = new double[mesh.Nodes.Count];
        for (var i = 0; i < rhs.Length; i++)
            rhs[i] = sys.Capacity.Diagonal(i) / dt * state.TemperatureKelvin[i];
        foreach (var source in sources)
        {
            var per = source.NodeIds.Count == 0 ? 0 : source.PowerWatts / source.NodeIds.Count;
            foreach (var n in source.NodeIds)
                rhs[n] += per;
        }
        var fixedDofs = boundaries
            .SelectMany(b =>
                b.NodeIds.Select(n => new ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof(
                    n,
                    b.TemperatureKelvin
                ))
            )
            .ToArray();
        var reduced = ApplyScalar(a, rhs, fixedDofs);
        var x = (double[])state.TemperatureKelvin.Clone();
        var report = _solver.Solve(reduced.Matrix, reduced.Rhs, x, ct);
        Array.Copy(x, state.TemperatureKelvin, x.Length);
        return report;
    }

    private static (CsrMatrix Matrix, double[] Rhs) ApplyScalar(
        CsrMatrix a,
        double[] rhs,
        IReadOnlyList<ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof> fixedDofs
    )
    {
        var fixedMap = fixedDofs.ToDictionary(x => x.Dof, x => x.Value);
        var t = new SparseTripletBuilder();
        var b = (double[])rhs.Clone();
        for (var r = 0; r < a.Rows; r++)
        {
            if (fixedMap.TryGetValue(r, out var v))
            {
                t.Add(r, r, 1);
                b[r] = v;
                continue;
            }
            for (var k = a.RowOffsets[r]; k < a.RowOffsets[r + 1]; k++)
            {
                var c = a.ColumnIndices[k];
                var q = a.Values[k];
                if (fixedMap.TryGetValue(c, out var u))
                    b[r] -= q * u;
                else
                    t.Add(r, c, q);
            }
        }
        return (t.Build(a.Rows, a.Columns), b);
    }
}
