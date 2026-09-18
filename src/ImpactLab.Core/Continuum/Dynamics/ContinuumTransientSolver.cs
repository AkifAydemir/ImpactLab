using System.Diagnostics;
using ImpactLab.Core.Continuum.Contact;
using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Continuum.Results;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class ContinuumTransientSolver
{
    public ContinuumDynamicResult Solve(
        TetrahedralMesh mesh,
        ScenarioDefinition scenario,
        IReadOnlyList<ContinuumDirichletDof> constraints,
        IReadOnlyList<RigidBodyDefinition> rigidDefinitions,
        PlasticityCatalog? plasticity = null,
        CancellationToken ct = default
    )
    {
        var settings = scenario.Continuum.Dynamic;
        settings.Validate();
        plasticity ??= PlasticityCatalog.CreateDefaults();
        var state = new ContinuumDynamicState(mesh.Nodes.Count, mesh.Elements.Count);
        var mass = ContinuumMassAssembler.Lumped(mesh);
        var projector = new ContinuumDynamicBoundaryProjector(constraints);
        ITransientContinuumIntegrator integrator =
            settings.Integrator == TimeIntegratorKind.NewmarkBeta
                ? new NewmarkBetaIntegrator()
                : new CentralDifferenceIntegrator();
        integrator.Begin(state, mass);
        var surface = TetrahedralSurfaceExtractor.Extract(mesh);
        var bodies = rigidDefinitions.Select(x => new RigidBodyState(x)).ToArray();
        var contact = new DynamicRigidSurfaceContact();
        var contactSettings = new DynamicSurfaceContactSettings();
        var frames = new List<ContinuumDynamicFrame>();
        var diag = new List<DynamicStepDiagnostics>();
        var sw = Stopwatch.StartNew();
        double externalWork = 0;
        for (var step = 0; step <= settings.StepCount; step++)
        {
            ct.ThrowIfCancellationRequested();
            var ext = ContinuumDynamicLoadEvaluator.Evaluate(mesh, scenario, state.TimeSeconds);
            var internalF = new double[ext.Length];
            foreach (var e in mesh.Elements)
                DynamicElementKernel.Accumulate(
                    mesh,
                    e,
                    state,
                    plasticity,
                    internalF,
                    settings.TimeStepSeconds
                );
            var net = new double[ext.Length];
            for (var i = 0; i < net.Length; i++)
            {
                net[i] =
                    ext[i]
                    - internalF[i]
                    - settings.Damping.MassCoefficient * VelocityDof(state, i) * mass[i];
            }
            var cdiag = settings.EnableDynamicContact
                ? contact.Apply(
                    mesh,
                    state,
                    surface,
                    bodies,
                    contactSettings,
                    net,
                    settings.TimeStepSeconds
                )
                : default;
            for (var i = 0; i < ext.Length; i++)
                externalWork += ext[i] * VelocityDof(state, i) * settings.TimeStepSeconds;
            integrator.Step(state, net, mass, settings.TimeStepSeconds, projector.Project);
            foreach (var b in bodies)
                b.Integrate(settings.TimeStepSeconds, settings.Damping.NumericalDamping);
            var kinetic = DynamicEnergyCalculator.Kinetic(state, mass);
            var internalE = DynamicEnergyCalculator.Internal(state);
            var plasticE = DynamicEnergyCalculator.Plastic(state);
            var d = new DynamicStepDiagnostics(
                state.TimeSeconds,
                kinetic,
                internalE,
                plasticE,
                cdiag.FrictionDissipationJ,
                externalWork,
                externalWork - (kinetic + internalE + plasticE + cdiag.FrictionDissipationJ),
                cdiag.ActivePairs,
                state.ElementStates.Count(x => x.Plastic.EquivalentPlasticStrain > 0)
            );
            diag.Add(d);
            if (step % settings.OutputStride == 0 || step == settings.StepCount)
                frames.Add(Frame(mesh, state, d));
        }
        sw.Stop();
        return new(mesh, frames, diag, sw.Elapsed);
    }

    private static double VelocityDof(ContinuumDynamicState s, int dof)
    {
        var v = s.Velocity[dof / 3];
        return dof
            % 3 switch
            {
                0 => v.X,
                1 => v.Y,
                _ => v.Z,
            };
    }

    private static ContinuumDynamicFrame Frame(
        TetrahedralMesh mesh,
        ContinuumDynamicState s,
        DynamicStepDiagnostics d
    )
    {
        var pos = mesh.Nodes.Select((n, i) => n.Position + s.Displacement[i]).ToArray();
        return new(
            s.TimeSeconds,
            pos,
            s.Displacement.ToArray(),
            s.Velocity.ToArray(),
            s.Acceleration.ToArray(),
            s.ElementStates.Select(x => x.TemperatureKelvin).ToArray(),
            s.ElementStates.Select(x => x.Stress).ToArray(),
            s.ElementStates.Select(x => x.TotalStrain).ToArray(),
            s.ElementStates.Select(x => x.Plastic.EquivalentPlasticStrain).ToArray(),
            d
        );
    }
}
