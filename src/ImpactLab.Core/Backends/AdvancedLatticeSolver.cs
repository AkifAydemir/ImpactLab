using System.Diagnostics;
using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Diagnostics;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Backends;

public sealed class AdvancedLatticeSolver
{
    private readonly AdvancedLatticeSettings _advanced;

    public AdvancedLatticeSolver(AdvancedLatticeSettings? settings = null) =>
        _advanced = settings ?? new();

    public SimulationResult Run(
        SimulationExecutionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var mesh = request.Compiled.Mesh;
        var settings = request.Settings;
        settings.Validate();
        var state = new SimulationState(mesh);
        var bodies = request.Compiled.RigidBodies.Select(x => new RigidBodyState(x)).ToArray();
        var surfaces = SurfaceNodeExtractor.Build(mesh);
        var contact = request.ContactSettings;
        contact.Validate();
        var runtime = new AdvancedSpringRuntime(mesh.Springs.Count);
        var frames = new List<SimulationFrame>();
        var telemetry = new SimulationTelemetrySeries();
        var reactionSeries = new ConstraintReactionSeries();
        var ctx = request.Context ?? new SimulationExecutionContext();
        var perf = ctx.Performance;
        var sw = Stopwatch.StartNew();
        var peakK = 0d;
        var peakE = 0d;
        var peakContact = 0;
        var maxPen = 0d;
        var peakN = 0d;
        var peakT = 0d;
        var friction = 0d;
        var maxDisp = 0d;
        foreach (var c in request.Compiled.Constraints)
            c.Apply(mesh, state);
        for (var step = 0; step <= settings.StepCount; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var time = step * settings.TimeStepSeconds;
            Clear(state);
            ApplyGravity(mesh, state, settings.Gravity);
            foreach (var b in bodies)
            {
                b.Force = settings.Gravity * b.Definition.MassKg;
                b.Torque = Vec3.Zero;
            }
            double elastic;
            using (
                perf.Measure(
                    PerformanceStage.InternalForces,
                    "advanced constitutive",
                    mesh.Springs.Count
                )
            )
                elastic = Internal(mesh, state, runtime, ctx, settings.TimeStepSeconds);
            foreach (var load in request.Compiled.Loads)
                load.Apply(time, settings.TimeStepSeconds, mesh, state);
            var pairs = new List<ContactPairMetrics>();
            var deformPairs = new List<DeformableContactMetrics>();
            var stepContacts = 0;
            var stepPen = 0d;
            var stepN = 0d;
            var stepT = 0d;
            var stepF = 0d;
            using (
                perf.Measure(
                    PerformanceStage.RigidContact,
                    "rigid contact",
                    bodies.Length * mesh.Parts.Count
                )
            )
                foreach (var body in bodies)
                foreach (var part in mesh.Parts)
                {
                    var rule = request.Compiled.ContactInteractions.Resolve(
                        body.Definition.Id,
                        part.PartId
                    );
                    var m = RigidContactSolver.Apply(
                        mesh,
                        state,
                        body,
                        part,
                        surfaces.Get(part.PartId),
                        contact,
                        rule,
                        settings.TimeStepSeconds
                    );
                    if (m.ContactCount > 0)
                        pairs.Add(m);
                    stepContacts += m.ContactCount;
                    stepPen = Math.Max(stepPen, m.MaxPenetrationMeters);
                    stepN += m.TotalNormalForceN;
                    stepT += m.TotalTangentialForceN;
                    stepF += m.DissipatedFrictionEnergyJ;
                }
            foreach (var k in request.Compiled.KinematicConstraints)
                k.Apply(time, mesh, state, KinematicConstraintPhase.PreForce);
            Integrate(mesh, state, settings);
            foreach (var k in request.Compiled.KinematicConstraints)
                k.Apply(
                    time + settings.TimeStepSeconds,
                    mesh,
                    state,
                    KinematicConstraintPhase.PostIntegrate
                );
            foreach (var b in bodies)
                IntegrateRigid(b, settings);
            friction += stepF;
            var kinetic = Kinetic(mesh, state);
            var rot = bodies.Sum(RotationalKineticEnergy);
            peakK = Math.Max(peakK, kinetic + rot);
            peakE = Math.Max(peakE, elastic);
            peakContact = Math.Max(peakContact, stepContacts);
            maxPen = Math.Max(maxPen, stepPen);
            peakN = Math.Max(peakN, stepN);
            peakT = Math.Max(peakT, stepT);
            maxDisp = Math.Max(maxDisp, MaxDisplacement(mesh, state));
            if (step % settings.TelemetryStride == 0)
                telemetry.Add(
                    new SimulationTelemetrySample(
                        time,
                        kinetic,
                        rot,
                        elastic,
                        maxDisp,
                        MaxNodeDamage(mesh),
                        mesh.Springs.Count(x => x.IsBroken),
                        stepContacts,
                        stepPen,
                        stepN,
                        stepT,
                        friction,
                        deformPairs.Sum(x => x.ContactCount)
                    )
                );
            if (step % settings.SnapshotStride == 0 || step == settings.StepCount)
                frames.Add(
                    Snapshot(
                        time,
                        mesh,
                        state,
                        bodies,
                        pairs,
                        deformPairs,
                        kinetic,
                        rot,
                        elastic,
                        stepContacts,
                        stepPen,
                        stepN,
                        stepT,
                        friction
                    )
                );
            if (
                ctx.CaptureNumericalHealth
                && _advanced.HealthStride > 0
                && step % _advanced.HealthStride == 0
            )
                ctx.HealthSink?.Invoke(
                    NumericalHealthMonitor.Evaluate(step, time, mesh, state, true)
                );
        }
        sw.Stop();
        return new SimulationResult(
            frames,
            telemetry,
            maxDisp,
            mesh.Springs.Count(x => x.IsBroken),
            peakK,
            peakE,
            peakContact,
            maxPen,
            peakN,
            peakT,
            friction,
            sw.Elapsed,
            reactionSeries
        );
    }

    private double Internal(
        SimulationMesh mesh,
        SimulationState state,
        AdvancedSpringRuntime runtime,
        SimulationExecutionContext ctx,
        double dt
    )
    {
        var energy = 0d;
        var materials = ctx.Materials;
        for (var i = 0; i < mesh.Springs.Count; i++)
        {
            var s = mesh.Springs[i];
            if (s.IsBroken)
                continue;
            var delta = state.Position[s.NodeB] - state.Position[s.NodeA];
            var length = delta.Length;
            if (length < 1e-12)
                continue;
            var dir = delta / length;
            var strain = (length - s.RestLength) / s.RestLength;
            var rel = state.Velocity[s.NodeB] - state.Velocity[s.NodeA];
            var strainRate = Vec3.Dot(rel, dir) / s.RestLength;
            double stress,
                tangent,
                drive;
            bool failed;
            if (materials is not null)
            {
                var profile = materials.Profiles.Get(s.Material.Id);
                var exec = materials.Resolve(s.Material.Id);
                var law = materials.Laws.Get(exec.ConstitutiveLawId);
                var ev = law.Evaluate(
                    profile,
                    new MaterialStatePoint(
                        strain,
                        strainRate,
                        runtime.TemperatureKelvin[i],
                        s.Damage
                    )
                );
                stress = ev.StressPa;
                tangent = ev.TangentModulusPa;
                drive = ev.DamageDrivingValue;
                failed = ev.Failed;
            }
            else
            {
                var ev = new LegacyBilinearConstitutiveLaw().Evaluate(
                    new MaterialProfile(s.Material),
                    new MaterialStatePoint(strain, strainRate, 293.15, s.Damage)
                );
                stress = ev.StressPa;
                tangent = ev.TangentModulusPa;
                drive = ev.DamageDrivingValue;
                failed = ev.Failed;
            }
            if (
                _advanced.EnableCohesiveInterfaces
                && materials is not null
                && s.IsMaterialInterface
            )
            {
                var a = mesh.Nodes[s.NodeA].Material.Id;
                var b = mesh.Nodes[s.NodeB].Material.Id;
                var cohesive = materials.ResolveCohesive(a, b);
                if (cohesive is not null)
                {
                    var cs = runtime.CohesiveStates[i] ??= new CohesiveInterfaceState();
                    var ce = CohesiveInterfaceEvaluator.Evaluate(
                        cohesive,
                        cs,
                        Math.Max(0, length - s.RestLength),
                        0,
                        s.EffectiveAreaM2,
                        dt
                    );
                    stress = ce.NormalTractionPa;
                    drive = Math.Max(drive, ce.Damage);
                    failed |= ce.Failed;
                }
            }
            var newDamage = failed
                ? 1.0
                : Math.Clamp(
                    Math.Max(s.Damage, drive),
                    s.Damage,
                    Math.Min(1.0, s.Damage + _advanced.DamageRateLimitPerStep)
                );
            s.Damage = newDamage;
            var force = stress * s.EffectiveAreaM2 * (1 - _advanced.NumericalViscosity);
            state.Force[s.NodeA] += dir * force;
            state.Force[s.NodeB] -= dir * force;
            energy +=
                0.5
                * Math.Max(tangent, 0)
                * strain
                * strain
                * s.EffectiveAreaM2
                * s.RestLength
                * (1 - s.Damage);
        }
        return energy;
    }

    private static void IntegrateRigid(RigidBodyState b, SimulationSettings settings)
    {
        if (b.Definition.IsKinematic)
        {
            b.Force = Vec3.Zero;
            b.Torque = Vec3.Zero;
            return;
        }
        var dt = settings.TimeStepSeconds;
        b.Velocity =
            (b.Velocity + b.Force / Math.Max(b.MassKg, 1e-12) * dt)
            * (1 - settings.GlobalVelocityDamping);
        b.PositionOffset += b.Velocity * dt;
        var localTorque = b.Orientation.InverseRotate(b.Torque);
        var localAlpha = b.Definition.InertiaBodyKgM2.InverseDiagonal() * localTorque;
        var worldAlpha = b.Orientation.Rotate(localAlpha);
        b.AngularVelocityRadPerSec =
            (b.AngularVelocityRadPerSec + worldAlpha * dt) * (1 - settings.AngularVelocityDamping);
        b.Orientation = (
            QuaternionD.FromAngularVelocity(b.AngularVelocityRadPerSec, dt) * b.Orientation
        ).Normalized();
        b.Force = Vec3.Zero;
        b.Torque = Vec3.Zero;
    }

    private static double RotationalKineticEnergy(RigidBodyState b)
    {
        var local = b.Orientation.InverseRotate(b.AngularVelocityRadPerSec);
        var iw = b.Definition.InertiaBodyKgM2 * local;
        return 0.5 * Vec3.Dot(local, iw);
    }

    private static void Clear(SimulationState s)
    {
        Array.Clear(s.Force, 0, s.Force.Length);
    }

    private static void ApplyGravity(SimulationMesh m, SimulationState s, Vec3 g)
    {
        for (var i = 0; i < m.Nodes.Count; i++)
            if (!s.IsFixed[i])
                s.Force[i] += g * m.Nodes[i].MassKg;
    }

    private static void Integrate(SimulationMesh m, SimulationState s, SimulationSettings set)
    {
        for (var i = 0; i < m.Nodes.Count; i++)
        {
            if (s.IsFixed[i])
            {
                s.Velocity[i] = Vec3.Zero;
                s.Position[i] = m.Nodes[i].RestPosition;
                continue;
            }
            var a = s.Force[i] / Math.Max(m.Nodes[i].MassKg, 1e-12);
            s.Velocity[i] =
                (s.Velocity[i] + a * set.TimeStepSeconds) * (1 - set.GlobalVelocityDamping);
            s.Position[i] += s.Velocity[i] * set.TimeStepSeconds;
        }
    }

    private static double Kinetic(SimulationMesh m, SimulationState s)
    {
        double e = 0;
        for (var i = 0; i < m.Nodes.Count; i++)
            e += 0.5 * m.Nodes[i].MassKg * s.Velocity[i].LengthSquared;
        return e;
    }

    private static double MaxDisplacement(SimulationMesh m, SimulationState s)
    {
        double v = 0;
        for (var i = 0; i < m.Nodes.Count; i++)
            v = Math.Max(v, (s.Position[i] - m.Nodes[i].RestPosition).Length);
        return v;
    }

    private static double MaxNodeDamage(SimulationMesh m) =>
        m.Springs.Count == 0 ? 0 : m.Springs.Max(x => x.Damage);

    private static SimulationFrame Snapshot(
        double time,
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<RigidBodyState> bodies,
        IReadOnlyList<ContactPairMetrics> pairs,
        IReadOnlyList<DeformableContactMetrics> deform,
        double k,
        double rk,
        double e,
        int cc,
        double pen,
        double nf,
        double tf,
        double friction
    )
    {
        var damage = new double[mesh.Nodes.Count];
        var counts = new int[mesh.Nodes.Count];
        foreach (var s in mesh.Springs)
        {
            damage[s.NodeA] += s.Damage;
            damage[s.NodeB] += s.Damage;
            counts[s.NodeA]++;
            counts[s.NodeB]++;
        }
        for (var i = 0; i < damage.Length; i++)
            if (counts[i] > 0)
                damage[i] /= counts[i];
        return new SimulationFrame(
            time,
            state.Position.ToArray(),
            state.Velocity.ToArray(),
            damage,
            k,
            rk,
            e,
            bodies
                .Select(x => new RigidBodyFrame(
                    x.Definition.Id,
                    x.PositionOffset,
                    x.Velocity,
                    x.Orientation,
                    x.AngularVelocityRadPerSec
                ))
                .ToArray(),
            pairs.ToArray(),
            deform.ToArray(),
            cc,
            pen,
            nf,
            tf,
            friction
        );
    }
}
