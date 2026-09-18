using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class NewtonRaphsonStepSolver
{
    private readonly ISparseLinearSolver _linear;
    private readonly NonlinearTransientSettings _settings;

    public NewtonRaphsonStepSolver(ISparseLinearSolver linear, NonlinearTransientSettings settings)
    {
        _linear = linear;
        _settings = settings;
    }

    public (double[] U, NonlinearStepMetrics Metrics) Solve(
        double time,
        double dt,
        double[] initial,
        Func<double[], NonlinearAssemblyResult> assemble,
        Func<NonlinearAssemblyResult, double[]> residual,
        NonlinearConstraintProjector constraints
    )
    {
        var u = (double[])initial.Clone();
        var hist = new List<NonlinearIterationMetrics>();
        double refNorm = 0,
            refInc = Math.Max(ResidualNormEvaluator.L2(u), 1);
        for (var it = 0; it < _settings.Convergence.MaxIterations; it++)
        {
            var a = assemble(u);
            var r = residual(a);
            constraints.ApplyToResidual(r);
            var rn = ResidualNormEvaluator.L2(r);
            if (it == 0)
                refNorm = Math.Max(rn, 1e-30);
            var du = _linear.Solve(a.Tangent, r.Select(x => -x).ToArray()).Solution;
            constraints.ApplyToIncrement(du);
            var inc = ResidualNormEvaluator.L2(du);
            var rr = rn / refNorm;
            var ri = inc / refInc;
            var conv =
                rr <= _settings.Convergence.ResidualRelative
                && ri <= _settings.Convergence.IncrementRelative;
            hist.Add(new(it, rn, rr, inc, ri, Math.Abs(a.InternalEnergy), 1, conv));
            if (conv)
                return (u, new(time, dt, it + 1, true, 0, hist));
            for (var i = 0; i < u.Length; i++)
                u[i] += du[i];
        }
        return (u, new(time, dt, hist.Count, false, 0, hist));
    }
}
