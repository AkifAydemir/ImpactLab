using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Static;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class IncrementalContinuumSolver
{
    private readonly LinearStaticContinuumSolver _linear = new();

    public IncrementalContinuumResult Solve(
        TetrahedralMesh mesh,
        double[] fullLoad,
        IReadOnlyList<ContinuumDirichletDof> constraints,
        ContinuumStaticSettings linear,
        NonlinearContinuumSettings settings,
        CancellationToken ct = default
    )
    {
        var steps = new List<ContinuumStaticResult>();
        var history = new List<NonlinearIteration>();
        var damage = new double[mesh.Elements.Count];
        for (var s = 1; s <= settings.LoadSteps; s++)
        {
            var factor = (double)s / settings.LoadSteps;
            var rhs = fullLoad.Select(x => x * factor).ToArray();
            ContinuumStaticResult? current = null;
            for (var it = 1; it <= settings.MaxNewtonIterations; it++)
            {
                ct.ThrowIfCancellationRequested();
                current = _linear.Solve(
                    mesh,
                    rhs,
                    constraints,
                    linear,
                    ct,
                    e => Math.Max(settings.MinimumStiffnessScale, 1.0 - damage[e.Id])
                );
                var peak = current.ElementStress.Max(x => x.VonMises);
                var updated = false;
                for (var i = 0; i < damage.Length; i++)
                {
                    var yield =
                        mesh.Elements[i].Material.YoungModulusPa
                        * mesh.Elements[i].Material.YieldStrain;
                    var ratio = yield <= 0 ? 0 : current.ElementStress[i].VonMises / yield;
                    var next = Math.Clamp((ratio - 1.0) * 0.08, 0, 0.98);
                    if (next > damage[i] + 1e-4)
                    {
                        damage[i] = next;
                        updated = true;
                    }
                }
                history.Add(new(s, it, factor, peak, (!updated)));
                if (!updated)
                    break;
            }
            if (current is not null)
                steps.Add(current with { LoadFactor = factor });
        }
        return new(steps, history);
    }
}
