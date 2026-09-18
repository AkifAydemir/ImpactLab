using ImpactLab.Core.Continuum;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal.Coupled;

public sealed class MonolithicThermoMechanicalSolver
{
    private readonly ISparseLinearSolver _solver;
    private readonly MonolithicCouplingSettings _s;

    public MonolithicThermoMechanicalSolver(
        ISparseLinearSolver solver,
        MonolithicCouplingSettings? settings = null
    )
    {
        _solver = solver;
        _s = settings ?? new();
        _s.Validate();
    }

    public MonolithicStepResult SolveStep(
        TetrahedralMesh mesh,
        IMonolithicThermoMechanicalAssembler assembler,
        CoupledState previous,
        double dt,
        CancellationToken ct = default
    )
    {
        var u = previous.Displacement.ToArray();
        var T = previous.TemperatureKelvin.ToArray();
        var history = new List<MonolithicNewtonIteration>();
        for (var it = 0; it < _s.MaxNewtonIterations; it++)
        {
            ct.ThrowIfCancellationRequested();
            var state = new CoupledState(
                u,
                T,
                previous.TimeSeconds + dt,
                previous.Velocity,
                previous.Acceleration
            );
            var a = assembler.Assemble(mesh, state, previous, dt, ct);
            if (a.Residual.CombinedNorm < _s.ResidualTolerance)
            {
                assembler.Commit();
                history.Add(
                    new(
                        it,
                        a.Residual.MechanicalNorm,
                        a.Residual.ThermalNorm,
                        a.Residual.CombinedNorm,
                        0,
                        1,
                        true
                    )
                );
                return new(
                    state,
                    true,
                    it + 1,
                    history,
                    a.MechanicalEnergyJ,
                    a.ThermalEnergyJ,
                    a.PlasticHeatJ
                );
            }
            var layout = new CoupledDofLayout(mesh.Nodes.Count);
            var K = CoupledBlockAssembler.Assemble(layout, a.Blocks);
            var rhs = Interleave(layout, a.Residual);
            var du = _solver.Solve(K, rhs, ct).Solution;
            var norm = Math.Sqrt(du.Sum(x => x * x));
            var scale = 1.0;
            Apply(layout, du, scale, u, T);
            history.Add(
                new(
                    it,
                    a.Residual.MechanicalNorm,
                    a.Residual.ThermalNorm,
                    a.Residual.CombinedNorm,
                    norm,
                    scale,
                    false
                )
            );
            if (norm < _s.IncrementTolerance)
            {
                assembler.Commit();
                return new(
                    new(u, T, previous.TimeSeconds + dt, previous.Velocity, previous.Acceleration),
                    true,
                    it + 1,
                    history,
                    a.MechanicalEnergyJ,
                    a.ThermalEnergyJ,
                    a.PlasticHeatJ
                );
            }
        }
        assembler.Revert();
        return new(
            new(u, T, previous.TimeSeconds + dt, previous.Velocity, previous.Acceleration),
            false,
            _s.MaxNewtonIterations,
            history,
            0,
            0,
            0
        );
    }

    static double[] Interleave(CoupledDofLayout l, CoupledResidual r)
    {
        var x = new double[l.TotalDofs];
        for (var n = 0; n < l.NodeCount; n++)
        {
            x[l.UX(n)] = -r.Mechanical[n * 3];
            x[l.UY(n)] = -r.Mechanical[n * 3 + 1];
            x[l.UZ(n)] = -r.Mechanical[n * 3 + 2];
            x[l.T(n)] = -r.Thermal[n];
        }
        return x;
    }

    static void Apply(CoupledDofLayout l, double[] dx, double s, double[] u, double[] T)
    {
        for (var n = 0; n < l.NodeCount; n++)
        {
            u[n * 3] += dx[l.UX(n)] * s;
            u[n * 3 + 1] += dx[l.UY(n)] * s;
            u[n * 3 + 2] += dx[l.UZ(n)] * s;
            T[n] += dx[l.T(n)] * s;
        }
    }
}
