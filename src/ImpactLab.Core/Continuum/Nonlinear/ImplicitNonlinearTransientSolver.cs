using ImpactLab.Core.Continuum.Dynamics;
using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class ImplicitNonlinearTransientSolver
{
    private readonly ISparseLinearSolver _linear;

    public ImplicitNonlinearTransientSolver(ISparseLinearSolver linear) => _linear = linear;

    public NonlinearTransientResult Solve(
        TetrahedralMesh mesh,
        NonlinearTransientSettings settings,
        INonlinearContinuumAssembler assembler,
        Func<double, double[]> externalForce,
        IReadOnlyList<int> fixedDofs,
        CancellationToken ct = default
    )
    {
        settings.Validate();
        var nd = mesh.Nodes.Count * 3;
        var u = new double[nd];
        var v = new double[nd];
        var a = new double[nd];
        var up = new double[nd];
        var vp = new double[nd];
        var committed = Enumerable
            .Repeat(Plasticity.PlasticState.Zero, mesh.Elements.Count)
            .ToArray();
        var mass = ContinuumMassMatrixFactory.LumpedCsr(mesh);
        var damping = ContinuumDampingMatrixFactory.Rayleigh(
            mass,
            ContinuumLinearStiffnessFactory.Elastic(mesh),
            settings.Damping
        );
        var frames = new List<NonlinearTransientFrame>();
        var steps = new List<NonlinearStepMetrics>();
        var projector = new NonlinearConstraintProjector(fixedDofs);
        double t = 0,
            dt = settings.TimeStepSeconds;
        while (t < settings.DurationSeconds - 1e-15)
        {
            ct.ThrowIfCancellationRequested();
            dt = Math.Min(dt, settings.DurationSeconds - t);
            NewmarkPredictor.Predict(
                u,
                v,
                a,
                dt,
                settings.NewmarkBeta,
                settings.NewmarkGamma,
                up,
                vp
            );
            var force = externalForce(t + dt);
            var initial = (double[])up.Clone();
            var solver = new NewtonRaphsonStepSolver(_linear, settings);
            var solved = solver.Solve(
                t + dt,
                dt,
                initial,
                x => assembler.Assemble(mesh, x, committed, dt, t + dt, ct),
                ar =>
                    DynamicResidualBuilder.Build(
                        ar,
                        mass,
                        damping,
                        x: initial,
                        predictor: up,
                        velocityPredictor: vp,
                        external: force,
                        dt: dt,
                        beta: settings.NewmarkBeta,
                        gamma: settings.NewmarkGamma
                    ),
                projector
            );
            if (!solved.Metrics.Converged)
            {
                if (
                    steps.Count(x => !x.Converged) >= settings.MaxCutbacks
                    || dt * 0.5 < settings.MinimumTimeStepSeconds
                )
                    throw new InvalidOperationException(
                        "Nonlinear transient step failed to converge."
                    );
                assembler.Revert();
                dt *= 0.5;
                continue;
            }
            u = solved.U;
            NewmarkCorrector.Correct(
                up,
                vp,
                u,
                dt,
                settings.NewmarkBeta,
                settings.NewmarkGamma,
                v,
                a
            );
            committed = assembler.Commit().ToArray();
            t += dt;
            steps.Add(solved.Metrics);
            if (steps.Count % settings.OutputStride == 0)
                frames.Add(NonlinearTransientFrame.Capture(mesh, t, u, v, a, committed));
        }
        return new(mesh, frames, steps);
    }
}
