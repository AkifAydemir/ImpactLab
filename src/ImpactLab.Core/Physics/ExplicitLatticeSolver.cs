using System.Diagnostics;
using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Diagnostics;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Physics;

public sealed class ExplicitLatticeSolver
{
    private readonly IDamageModel _damageModel;

    public ExplicitLatticeSolver(IDamageModel damageModel)
    {
        _damageModel = damageModel;
    }

    public SimulationResult Run(
        SimulationMesh mesh,
        SimulationSettings settings,
        IReadOnlyList<ILoadSource> loads,
        IReadOnlyList<IFixedConstraint> constraints,
        IReadOnlyList<RigidBodyDefinition>? rigidBodies = null,
        ContactSettings? contactSettings = null,
        ContactInteractionTable? contactInteractions = null,
        DeformableContactTable? deformableInteractions = null,
        CancellationToken cancellationToken = default,
        IReadOnlyList<IKinematicConstraint>? kinematicConstraints = null
    )
    {
        settings.Validate();
        var stability = StabilityEstimator.Estimate(mesh, settings.TimeStepSeconds);
        if (settings.EnforceStabilityEstimate && !stability.IsLikelyStable)
            throw new InvalidOperationException(stability.Message);
        var state = new SimulationState(mesh);
        var bodies = (rigidBodies ?? []).Select(x => new RigidBodyState(x)).ToArray();
        var contact = contactSettings ?? ContactSettings.Default;
        var rigidRules = contactInteractions ?? new ContactInteractionTable();
        var deformableRules = deformableInteractions ?? new DeformableContactTable();
        var kinematicRules = kinematicConstraints ?? Array.Empty<IKinematicConstraint>();
        var surfaces = SurfaceNodeExtractor.Build(mesh);
        var deformableContactSolver = new DeformableContactSolver(mesh.CellSize * 2.5);
        contact.Validate();
        foreach (var constraint in constraints)
            constraint.Apply(mesh, state);
        var frames = new List<SimulationFrame>();
        var telemetry = new SimulationTelemetrySeries();
        var stopwatch = Stopwatch.StartNew();
        var maxDisplacement = 0.0;
        var peakKinetic = 0.0;
        var peakElastic = 0.0;
        var peakContactCount = 0;
        var maxContactPenetration = 0.0;
        var peakContactForce = 0.0;
        var peakTangentialForce = 0.0;
        var frictionEnergyTotal = 0.0;
        var reactionSeries = new ConstraintReactionSeries();
        for (var step = 0; step <= settings.StepCount; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var time = step * settings.TimeStepSeconds;
            foreach (var boundary in kinematicRules)
                boundary.Apply(time, mesh, state, KinematicConstraintPhase.PreForce);
            ClearForces(state);
            ApplyGravity(mesh, settings, state);
            ClearRigidForces(bodies, settings.Gravity);
            var elasticEnergy = AccumulateInternalForces(mesh, state, settings.TimeStepSeconds);
            foreach (var load in loads)
                load.Apply(time, settings.TimeStepSeconds, mesh, state);
            var pairMetrics = new List<ContactPairMetrics>();
            var deformableMetrics = new List<DeformableContactMetrics>();
            var contactCount = 0;
            var stepMaxPenetration = 0.0;
            var stepNormalForce = 0.0;
            var stepTangentialForce = 0.0;
            var stepFrictionEnergy = 0.0;
            foreach (var body in bodies)
            {
                foreach (var part in mesh.Parts)
                {
                    var rule = rigidRules.Resolve(body.Definition.Id, part.PartId);
                    rule.Validate();
                    var metrics = RigidContactSolver.Apply(
                        mesh,
                        state,
                        body,
                        part,
                        surfaces.Get(part.PartId),
                        contact,
                        rule,
                        settings.TimeStepSeconds
                    );
                    if (!rule.Enabled)
                        continue;
                    pairMetrics.Add(metrics);
                    contactCount += metrics.ContactCount;
                    stepMaxPenetration = Math.Max(stepMaxPenetration, metrics.MaxPenetrationMeters);
                    stepNormalForce += metrics.TotalNormalForceN;
                    stepTangentialForce += metrics.TotalTangentialForceN;
                    stepFrictionEnergy += metrics.DissipatedFrictionEnergyJ;
                }
            }
            if (settings.EnableDeformableContact)
            {
                for (var a = 0; a < mesh.Parts.Count; a++)
                for (var b = a + 1; b < mesh.Parts.Count; b++)
                {
                    var partA = mesh.Parts[a];
                    var partB = mesh.Parts[b];
                    var rule = deformableRules.Resolve(partA.PartId, partB.PartId);
                    var metrics = deformableContactSolver.Apply(
                        mesh,
                        state,
                        surfaces.Get(partA.PartId),
                        surfaces.Get(partB.PartId),
                        partA.PartId,
                        partB.PartId,
                        contact,
                        rule
                    );
                    if (metrics.ContactCount > 0)
                    {
                        deformableMetrics.Add(metrics);
                        contactCount += metrics.ContactCount;
                        stepNormalForce += metrics.TotalNormalForceN;
                    }
                }
            }
            Integrate(mesh, settings, state);
            foreach (var boundary in kinematicRules)
            {
                var velocityBeforeProjection = state.Velocity.ToArray();
                boundary.Apply(
                    time + settings.TimeStepSeconds,
                    mesh,
                    state,
                    KinematicConstraintPhase.PostIntegrate
                );
                var reaction = ConstraintReactionRecorder.Measure(
                    boundary.Id,
                    time + settings.TimeStepSeconds,
                    mesh,
                    velocityBeforeProjection,
                    state,
                    settings.TimeStepSeconds
                );
                if (reaction.CorrectedNodeCount > 0)
                    reactionSeries.Add(reaction);
            }
            IntegrateRigidBodies(bodies, settings);
            var translationalKinetic = CalculateTranslationalKineticEnergy(mesh, state, bodies);
            var rotationalKinetic = CalculateRotationalKineticEnergy(bodies);
            var currentMaxDisplacement = CalculateMaxDisplacement(mesh, state);
            maxDisplacement = Math.Max(maxDisplacement, currentMaxDisplacement);
            peakKinetic = Math.Max(peakKinetic, translationalKinetic + rotationalKinetic);
            peakElastic = Math.Max(peakElastic, elasticEnergy);
            peakContactCount = Math.Max(peakContactCount, contactCount);
            maxContactPenetration = Math.Max(maxContactPenetration, stepMaxPenetration);
            peakContactForce = Math.Max(peakContactForce, stepNormalForce);
            peakTangentialForce = Math.Max(peakTangentialForce, stepTangentialForce);
            frictionEnergyTotal += stepFrictionEnergy;
            var captureFrame = step % settings.SnapshotStride == 0 || step == settings.StepCount;
            var captureTelemetry =
                step % settings.TelemetryStride == 0 || step == settings.StepCount;
            double[]? nodeDamage = null;
            if (captureFrame || captureTelemetry)
                nodeDamage = CalculateNodeDamage(mesh);
            if (captureTelemetry)
            {
                telemetry.Add(
                    new SimulationTelemetrySample(
                        time,
                        translationalKinetic,
                        rotationalKinetic,
                        elasticEnergy,
                        currentMaxDisplacement,
                        nodeDamage!.Length == 0 ? 0.0 : nodeDamage.Max(),
                        mesh.Springs.Count(x => x.IsBroken),
                        contactCount,
                        stepMaxPenetration,
                        stepNormalForce,
                        stepTangentialForce,
                        frictionEnergyTotal,
                        deformableMetrics.Sum(x => x.ContactCount)
                    )
                );
            }
            if (captureFrame)
            {
                frames.Add(
                    CreateFrame(
                        state,
                        bodies,
                        time,
                        translationalKinetic,
                        rotationalKinetic,
                        elasticEnergy,
                        nodeDamage!,
                        pairMetrics,
                        deformableMetrics,
                        contactCount,
                        stepMaxPenetration,
                        stepNormalForce,
                        stepTangentialForce,
                        frictionEnergyTotal
                    )
                );
            }
        }
        stopwatch.Stop();
        return new SimulationResult(
            frames,
            telemetry,
            maxDisplacement,
            mesh.Springs.Count(x => x.IsBroken),
            peakKinetic,
            peakElastic,
            peakContactCount,
            maxContactPenetration,
            peakContactForce,
            peakTangentialForce,
            frictionEnergyTotal,
            stopwatch.Elapsed,
            reactionSeries
        );
    }

    private double AccumulateInternalForces(SimulationMesh mesh, SimulationState state, double dt)
    {
        var elasticEnergy = 0.0;
        foreach (var spring in mesh.Springs)
        {
            if (spring.IsBroken)
                continue;
            var a = spring.NodeA;
            var b = spring.NodeB;
            var delta = state.Position[b] - state.Position[a];
            var length = delta.Length;
            if (length <= 1e-12)
                continue;
            var direction = delta / length;
            var extension = length - spring.RestLength;
            var strain = extension / spring.RestLength;
            var material = spring.Material;
            spring.Damage = _damageModel.UpdateDamage(spring, spring.Damage, strain, dt);
            if (spring.IsBroken)
                continue;
            var stiffnessScale = spring.Interface?.StiffnessScale ?? 1.0;
            var dampingScale = spring.Interface?.DampingScale ?? 1.0;
            var baseStiffness =
                material.YoungModulusPa * spring.EffectiveAreaM2 / spring.RestLength;
            var yielded = Math.Abs(strain) > material.YieldStrain;
            var tangent =
                material.TangentModulusPa > 0.0
                    ? material.TangentModulusPa
                    : material.YoungModulusPa * 0.02;
            var stiffness =
                (yielded ? tangent : material.YoungModulusPa)
                * spring.EffectiveAreaM2
                / spring.RestLength
                * stiffnessScale;
            var relativeVelocity = state.Velocity[b] - state.Velocity[a];
            var relativeAlongSpring = Vec3.Dot(relativeVelocity, direction);
            var effectiveMass =
                (mesh.Nodes[a].MassKg * mesh.Nodes[b].MassKg)
                / Math.Max(mesh.Nodes[a].MassKg + mesh.Nodes[b].MassKg, 1e-18);
            var criticalDamping = 2.0 * Math.Sqrt(Math.Max(stiffness * effectiveMass, 0.0));
            var damping = material.DampingRatio * criticalDamping * dampingScale;
            var damageScale = 1.0 - spring.Damage;
            var forceMagnitude =
                (stiffness * extension + damping * relativeAlongSpring) * damageScale;
            var force = direction * forceMagnitude;
            state.Force[a] += force;
            state.Force[b] -= force;
            elasticEnergy += 0.5 * baseStiffness * extension * extension * damageScale;
        }
        return elasticEnergy;
    }

    private static void Integrate(
        SimulationMesh mesh,
        SimulationSettings settings,
        SimulationState state
    )
    {
        var dt = settings.TimeStepSeconds;
        var dampingScale = Math.Clamp(1.0 - settings.GlobalVelocityDamping, 0.0, 1.0);
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (state.IsFixed[i])
            {
                state.Position[i] = mesh.Nodes[i].RestPosition;
                state.Velocity[i] = Vec3.Zero;
                continue;
            }
            var acceleration = state.Force[i] / mesh.Nodes[i].MassKg;
            state.Velocity[i] = (state.Velocity[i] + acceleration * dt) * dampingScale;
            state.Position[i] += state.Velocity[i] * dt;
        }
    }

    private static void IntegrateRigidBodies(
        IReadOnlyList<RigidBodyState> bodies,
        SimulationSettings settings
    )
    {
        var dt = settings.TimeStepSeconds;
        var angularDamping = Math.Clamp(1.0 - settings.AngularVelocityDamping, 0.0, 1.0);
        foreach (var body in bodies)
        {
            if (body.Definition.IsKinematic)
                continue;
            body.Velocity += body.Force / body.MassKg * dt;
            body.PositionOffset += body.Velocity * dt;
            var localTorque = body.Orientation.InverseRotate(body.Torque);
            var localAngularAcceleration =
                body.Definition.InertiaBodyKgM2.InverseDiagonal() * localTorque;
            var worldAngularAcceleration = body.Orientation.Rotate(localAngularAcceleration);
            body.AngularVelocityRadPerSec =
                (body.AngularVelocityRadPerSec + worldAngularAcceleration * dt) * angularDamping;
            body.Orientation = (
                QuaternionD.FromAngularVelocity(body.AngularVelocityRadPerSec, dt)
                * body.Orientation
            ).Normalized();
        }
    }

    private static void ClearForces(SimulationState state) => Array.Fill(state.Force, Vec3.Zero);

    private static void ClearRigidForces(IReadOnlyList<RigidBodyState> bodies, Vec3 gravity)
    {
        foreach (var body in bodies)
        {
            body.Force = gravity * body.MassKg;
            body.Torque = Vec3.Zero;
        }
    }

    private static void ApplyGravity(
        SimulationMesh mesh,
        SimulationSettings settings,
        SimulationState state
    )
    {
        if (settings.Gravity.LengthSquared <= 1e-12)
            return;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            state.Force[i] += settings.Gravity * mesh.Nodes[i].MassKg;
    }

    private static double CalculateTranslationalKineticEnergy(
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<RigidBodyState> bodies
    )
    {
        var energy = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            energy += 0.5 * mesh.Nodes[i].MassKg * state.Velocity[i].LengthSquared;
        foreach (var body in bodies)
            energy += 0.5 * body.MassKg * body.Velocity.LengthSquared;
        return energy;
    }

    private static double CalculateRotationalKineticEnergy(IReadOnlyList<RigidBodyState> bodies)
    {
        var energy = 0.0;
        foreach (var body in bodies)
        {
            var localOmega = body.Orientation.InverseRotate(body.AngularVelocityRadPerSec);
            var angularMomentumLocal = body.Definition.InertiaBodyKgM2 * localOmega;
            energy += 0.5 * Vec3.Dot(localOmega, angularMomentumLocal);
        }
        return energy;
    }

    private static double CalculateMaxDisplacement(SimulationMesh mesh, SimulationState state)
    {
        var max = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            max = Math.Max(max, (state.Position[i] - mesh.Nodes[i].RestPosition).Length);
        return max;
    }

    private static SimulationFrame CreateFrame(
        SimulationState state,
        IReadOnlyList<RigidBodyState> bodies,
        double time,
        double kinetic,
        double rotational,
        double elastic,
        double[] nodeDamage,
        IReadOnlyList<ContactPairMetrics> pairMetrics,
        IReadOnlyList<DeformableContactMetrics> deformableMetrics,
        int contactCount,
        double maxPenetration,
        double normalForce,
        double tangentialForce,
        double frictionEnergy
    )
    {
        var rigidFrames = bodies
            .Select(x => new RigidBodyFrame(
                x.Definition.Id,
                x.PositionOffset,
                x.Velocity,
                x.Orientation,
                x.AngularVelocityRadPerSec
            ))
            .ToArray();
        return new SimulationFrame(
            time,
            (Vec3[])state.Position.Clone(),
            (Vec3[])state.Velocity.Clone(),
            nodeDamage,
            kinetic,
            rotational,
            elastic,
            rigidFrames,
            pairMetrics.ToArray(),
            deformableMetrics.ToArray(),
            contactCount,
            maxPenetration,
            normalForce,
            tangentialForce,
            frictionEnergy
        );
    }

    private static double[] CalculateNodeDamage(SimulationMesh mesh)
    {
        var damageSum = new double[mesh.Nodes.Count];
        var count = new int[mesh.Nodes.Count];
        foreach (var spring in mesh.Springs)
        {
            damageSum[spring.NodeA] += spring.Damage;
            damageSum[spring.NodeB] += spring.Damage;
            count[spring.NodeA]++;
            count[spring.NodeB]++;
        }
        for (var i = 0; i < damageSum.Length; i++)
            damageSum[i] = count[i] == 0 ? 0.0 : damageSum[i] / count[i];
        return damageSum;
    }
}
