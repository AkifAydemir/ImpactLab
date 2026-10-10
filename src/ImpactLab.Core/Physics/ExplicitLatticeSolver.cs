using System.Diagnostics;
using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Diagnostics;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Physics;

public sealed class ExplicitLatticeSolver
{
    private readonly IDamageModel _damageModel;
    private readonly IRigidContactModel _rigidContactModel;
    private readonly IDeformableContactModelFactory _deformableContactFactory;
    private readonly ILatticeConstitutiveModel _constitutiveModel;
    private readonly IFragmentTopologyModel _fragmentTopologyModel;

    public ExplicitLatticeSolver(
        IDamageModel damageModel,
        IRigidContactModel? rigidContactModel = null,
        IDeformableContactModelFactory? deformableContactFactory = null,
        ILatticeConstitutiveModel? constitutiveModel = null,
        IFragmentTopologyModel? fragmentTopologyModel = null
    )
    {
        _damageModel = damageModel;
        _rigidContactModel = rigidContactModel ?? new PenaltyRigidContactModel();
        _deformableContactFactory =
            deformableContactFactory ?? new PenaltyDeformableContactModelFactory();
        _constitutiveModel = constitutiveModel ?? new LegacyLatticeConstitutiveModel();
        _fragmentTopologyModel =
            fragmentTopologyModel ?? new ConnectedLatticeFragmentTopologyModel();
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
        var fragmentRepresentationPolicy = new FragmentRepresentationPolicy(
            settings.FragmentRepresentation
        );
        var stability = StabilityEstimator.Estimate(mesh, settings.TimeStepSeconds);
        if (settings.EnforceStabilityEstimate && !stability.IsLikelyStable)
            throw new InvalidOperationException(stability.Message);
        var state = new SimulationState(mesh);
        var bodies = (rigidBodies ?? []).Select(x => new RigidBodyState(x)).ToArray();
        var fragmentLifecycle = new FragmentLifecycleTracker();
        var fragmentLineage = new FragmentLineageTracker();
        var contact = contactSettings ?? ContactSettings.Default;
        var rigidRules = contactInteractions ?? new ContactInteractionTable();
        var deformableRules = deformableInteractions ?? new DeformableContactTable();
        var kinematicRules = kinematicConstraints ?? Array.Empty<IKinematicConstraint>();
        var surfaces = SurfaceNodeExtractor.Build(mesh);
        var deformableContactSolver = _deformableContactFactory.Create(mesh.CellSize * 2.5);
        var coarseContactSolver = new CoarseFragmentContactSolver();
        contact.Validate();
        foreach (var constraint in constraints)
            constraint.Apply(mesh, state);
        fragmentLineage.Observe(_fragmentTopologyModel.Analyze(mesh, state), 0, 0.0);
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
        var contactDampingEnergyTotal = 0.0;
        var structuralDampingEnergyTotal = 0.0;
        var globalDampingEnergyTotal = 0.0;
        var plasticDissipationEnergyTotal = 0.0;
        var fractureDissipationEnergyTotal = 0.0;
        var externalWorkTotal = 0.0;
        var constraintWorkTotal = 0.0;
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
            var internalForces = AccumulateInternalForces(mesh, state, settings.TimeStepSeconds);
            var elasticEnergy = internalForces.ElasticEnergyJ;
            var fragmentClusters = _fragmentTopologyModel.Analyze(mesh, state);
            var fragmentBirths = fragmentLineage.Observe(fragmentClusters, step, time);
            var fragmentRepresentations = fragmentRepresentationPolicy.DecideAll(fragmentClusters);
            var fragmentCoarseStates = FragmentStateTransfer.ProjectCoarseStates(
                fragmentClusters,
                fragmentRepresentations
            );
            var transferAudit = FragmentStateTransfer.AuditCoarseProjection(
                fragmentClusters,
                fragmentCoarseStates
            );
            if (settings.EnableCoarseFragmentIntegration)
            {
                var spawned = fragmentLifecycle.SpawnNew(fragmentCoarseStates);
                foreach (var fragment in spawned)
                foreach (var nodeId in fragment.Source.SourceNodeIds)
                {
                    state.IsActive[nodeId] = false;
                    state.Force[nodeId] = Vec3.Zero;
                }
                fragmentCoarseStates = fragmentLifecycle.States.Select(x => x.Snapshot()).ToArray();
            }
            foreach (var load in loads)
                load.Apply(time, settings.TimeStepSeconds, mesh, state);
            ClearInactiveForces(state);
            var pairMetrics = new List<ContactPairMetrics>();
            var deformableMetrics = new List<DeformableContactMetrics>();
            var coarseContactMetrics = new List<CoarseFragmentContactMetrics>();
            var contactCount = 0;
            var stepMaxPenetration = 0.0;
            var stepNormalForce = 0.0;
            var stepTangentialForce = 0.0;
            var stepFrictionEnergy = 0.0;
            var stepContactPotentialEnergy = 0.0;
            var stepContactDampingEnergy = 0.0;
            var stepSecondaryFragmentContactCount = 0;
            coarseContactSolver.BeginStep();
            foreach (var body in bodies)
            {
                foreach (var part in mesh.Parts)
                {
                    var rule = rigidRules.Resolve(body.Definition.Id, part.PartId);
                    rule.Validate();
                    var metrics = _rigidContactModel.Apply(
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
                    stepContactPotentialEnergy += metrics.StoredNormalPenaltyEnergyJ;
                    stepContactDampingEnergy += metrics.DissipatedNormalDampingEnergyJ;
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
                        rule,
                        settings.TimeStepSeconds
                    );
                    if (metrics.ContactCount > 0)
                    {
                        deformableMetrics.Add(metrics);
                        contactCount += metrics.ContactCount;
                        stepNormalForce += metrics.TotalNormalForceN;
                        stepContactPotentialEnergy += metrics.StoredNormalPenaltyEnergyJ;
                        stepContactDampingEnergy += metrics.DissipatedNormalDampingEnergyJ;
                    }
                }
            }
            if (settings.EnableFragmentSelfContact && fragmentClusters.Count > 1)
            {
                foreach (var partGroup in fragmentClusters.GroupBy(x => x.PartId))
                {
                    var fragments = partGroup.ToArray();
                    if (fragments.Length < 2)
                        continue;
                    var rule = deformableRules.Resolve(partGroup.Key, partGroup.Key);
                    for (var a = 0; a < fragments.Length; a++)
                    for (var b = a + 1; b < fragments.Length; b++)
                    {
                        var metrics = deformableContactSolver.Apply(
                            mesh,
                            state,
                            fragments[a].NodeIds,
                            fragments[b].NodeIds,
                            fragments[a].Id,
                            fragments[b].Id,
                            contact,
                            rule,
                            settings.TimeStepSeconds
                        );
                        if (metrics.ContactCount == 0)
                            continue;
                        deformableMetrics.Add(metrics);
                        contactCount += metrics.ContactCount;
                        stepSecondaryFragmentContactCount += metrics.ContactCount;
                        stepMaxPenetration = Math.Max(stepMaxPenetration, metrics.MaxOverlapMeters);
                        stepNormalForce += metrics.TotalNormalForceN;
                        stepContactPotentialEnergy += metrics.StoredNormalPenaltyEnergyJ;
                        stepContactDampingEnergy += metrics.DissipatedNormalDampingEnergyJ;
                    }
                }
            }
            if (settings.EnableCoarseFragmentIntegration)
            {
                var coarseBodies = fragmentLifecycle.States.ToArray();
                foreach (var fragment in coarseBodies)
                foreach (var part in mesh.Parts)
                {
                    var rule = deformableRules.Resolve(fragment.PartId, part.PartId);
                    AccumulateCoarseContact(
                        coarseContactSolver.ApplyResolved(
                            mesh,
                            state,
                            fragment,
                            surfaces.Get(part.PartId),
                            part.PartId,
                            contact,
                            rule,
                            settings.TimeStepSeconds
                        )
                    );
                }
                for (var a = 0; a < coarseBodies.Length; a++)
                for (var b = a + 1; b < coarseBodies.Length; b++)
                {
                    var rule = deformableRules.Resolve(
                        coarseBodies[a].PartId,
                        coarseBodies[b].PartId
                    );
                    AccumulateCoarseContact(
                        coarseContactSolver.ApplyPair(
                            coarseBodies[a],
                            coarseBodies[b],
                            contact,
                            rule,
                            settings.TimeStepSeconds
                        )
                    );
                }
            }
            coarseContactSolver.EndStep();
            var gravityCoordinateBefore = GravityWorkCoordinate(
                mesh,
                state,
                bodies,
                fragmentLifecycle.States,
                settings.Gravity
            );
            var stepGlobalDampingEnergy = Integrate(mesh, settings, state);
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
            stepGlobalDampingEnergy += IntegrateRigidBodies(bodies, settings);
            if (settings.EnableCoarseFragmentIntegration)
                fragmentLifecycle.IntegrateAll(settings.TimeStepSeconds, settings.Gravity);
            var gravityCoordinateAfter = GravityWorkCoordinate(
                mesh,
                state,
                bodies,
                fragmentLifecycle.States,
                settings.Gravity
            );
            if (settings.EnableCoarseFragmentIntegration)
                fragmentCoarseStates = fragmentLifecycle.States.Select(x => x.Snapshot()).ToArray();
            var translationalKinetic = CalculateTranslationalKineticEnergy(
                mesh,
                state,
                bodies,
                fragmentLifecycle.States
            );
            var rotationalKinetic = CalculateRotationalKineticEnergy(
                bodies,
                fragmentLifecycle.States
            );
            var currentMaxDisplacement = CalculateMaxDisplacement(mesh, state);
            maxDisplacement = Math.Max(maxDisplacement, currentMaxDisplacement);
            peakKinetic = Math.Max(peakKinetic, translationalKinetic + rotationalKinetic);
            peakElastic = Math.Max(peakElastic, elasticEnergy);
            peakContactCount = Math.Max(peakContactCount, contactCount);
            maxContactPenetration = Math.Max(maxContactPenetration, stepMaxPenetration);
            peakContactForce = Math.Max(peakContactForce, stepNormalForce);
            peakTangentialForce = Math.Max(peakTangentialForce, stepTangentialForce);
            frictionEnergyTotal += stepFrictionEnergy;
            contactDampingEnergyTotal += stepContactDampingEnergy;
            structuralDampingEnergyTotal += internalForces.StructuralDampingEnergyJ;
            globalDampingEnergyTotal += stepGlobalDampingEnergy;
            plasticDissipationEnergyTotal += internalForces.PlasticDissipationEnergyJ;
            fractureDissipationEnergyTotal += internalForces.FractureDissipationEnergyJ;
            externalWorkTotal += gravityCoordinateAfter - gravityCoordinateBefore;
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
                        deformableMetrics.Sum(x => x.ContactCount),
                        stepContactPotentialEnergy,
                        contactDampingEnergyTotal,
                        PlasticDissipationEnergyJ: plasticDissipationEnergyTotal,
                        FractureDissipationEnergyJ: fractureDissipationEnergyTotal,
                        ExternalWorkJ: externalWorkTotal,
                        ConstraintWorkJ: constraintWorkTotal,
                        StructuralDampingEnergyJ: structuralDampingEnergyTotal,
                        GlobalDampingEnergyJ: globalDampingEnergyTotal,
                        FragmentClusterCount: settings.EnableCoarseFragmentIntegration
                            ? fragmentRepresentations.Count(x =>
                                x.Kind == FragmentRepresentationKind.ResolvedDeformable
                            ) + fragmentCoarseStates.Count
                            : fragmentClusters.Count,
                        DetachedFragmentMassKg: settings.EnableCoarseFragmentIntegration
                            ? fragmentCoarseStates.Sum(x => x.MassKg)
                                + FragmentClusterAnalyzer
                                    .Detached(fragmentClusters)
                                    .Where(x =>
                                        fragmentRepresentations
                                            .First(y => y.FragmentId == x.Id)
                                            .Kind == FragmentRepresentationKind.ResolvedDeformable
                                    )
                                    .Sum(x => x.MassKg)
                            : FragmentClusterAnalyzer.Detached(fragmentClusters).Sum(x => x.MassKg),
                        SecondaryFragmentContactCount: stepSecondaryFragmentContactCount,
                        ResolvedFragmentCount: fragmentRepresentations.Count(x =>
                            x.Kind == FragmentRepresentationKind.ResolvedDeformable
                        ),
                        ReducedOrderFragmentCount: fragmentCoarseStates.Count(x =>
                            x.Representation == FragmentRepresentationKind.ReducedOrderRigid
                        ),
                        StatisticalFragmentCount: fragmentCoarseStates.Count(x =>
                            x.Representation == FragmentRepresentationKind.StatisticalParticle
                        ),
                        CoarseFragmentKineticEnergyJ: fragmentCoarseStates.Sum(x =>
                            x.AccountedKineticEnergyJ
                        ),
                        CoarseFragmentUnresolvedKineticEnergyJ: fragmentCoarseStates.Sum(x =>
                            x.UnresolvedInternalKineticEnergyJ
                        ),
                        CoarseFragmentContactCount: coarseContactMetrics.Sum(x => x.ContactCount),
                        MinimumCoarseContactCriticalTimeStepSeconds: coarseContactMetrics.Count == 0
                            ? double.PositiveInfinity
                            : coarseContactMetrics.Min(x => x.CriticalTimeStepSeconds),
                        CoarseContactMomentumResidualN: coarseContactMetrics.Sum(x =>
                            x.MomentumBalanceResidualN
                        ),
                        FragmentBirthCount: fragmentBirths.Count,
                        MaximumFragmentGeneration: fragmentLineage.Records.Count == 0
                            ? 0
                            : fragmentLineage.Records.Max(x => x.Generation)
                    )
                );
            }
            if (captureFrame)
            {
                frames.Add(
                    CreateFrame(
                        mesh,
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
                        frictionEnergyTotal,
                        stepContactPotentialEnergy,
                        contactDampingEnergyTotal,
                        structuralDampingEnergyTotal,
                        globalDampingEnergyTotal,
                        plasticDissipationEnergyTotal,
                        fractureDissipationEnergyTotal,
                        externalWorkTotal,
                        constraintWorkTotal,
                        stepSecondaryFragmentContactCount,
                        _fragmentTopologyModel,
                        fragmentRepresentationPolicy,
                        fragmentCoarseStates,
                        transferAudit,
                        coarseContactMetrics,
                        fragmentLineage.Records.ToArray()
                    )
                );
            }

            void AccumulateCoarseContact(CoarseFragmentContactMetrics metrics)
            {
                if (metrics.ContactCount == 0)
                    return;
                if (
                    settings.EnforceStabilityEstimate
                    && settings.TimeStepSeconds > metrics.CriticalTimeStepSeconds
                )
                    throw new InvalidOperationException(
                        $"Coarse fragment contact requires dt <= {metrics.CriticalTimeStepSeconds:G6} s; configured dt is {settings.TimeStepSeconds:G6} s."
                    );
                coarseContactMetrics.Add(metrics);
                contactCount += metrics.ContactCount;
                stepSecondaryFragmentContactCount += metrics.ContactCount;
                stepMaxPenetration = Math.Max(stepMaxPenetration, metrics.MaxOverlapMeters);
                stepNormalForce += metrics.TotalNormalForceN;
                stepTangentialForce += metrics.TotalTangentialForceN;
                stepFrictionEnergy += metrics.DissipatedFrictionEnergyJ;
                stepContactPotentialEnergy +=
                    metrics.StoredNormalPenaltyEnergyJ + metrics.StoredTangentialPenaltyEnergyJ;
                stepContactDampingEnergy += metrics.DissipatedNormalDampingEnergyJ;
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

    private readonly record struct InternalForceMetrics(
        double ElasticEnergyJ,
        double StructuralDampingEnergyJ,
        double PlasticDissipationEnergyJ,
        double FractureDissipationEnergyJ
    );

    private InternalForceMetrics AccumulateInternalForces(
        SimulationMesh mesh,
        SimulationState state,
        double dt
    )
    {
        var elasticEnergy = 0.0;
        var structuralDampingEnergy = 0.0;
        var plasticDissipationEnergy = 0.0;
        var fractureDissipationEnergy = 0.0;
        foreach (var spring in mesh.Springs)
        {
            if (spring.IsBroken || !state.IsActive[spring.NodeA] || !state.IsActive[spring.NodeB])
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
            var damageBefore = spring.Damage;
            spring.Damage = _damageModel.UpdateDamage(spring, spring.Damage, strain, dt);
            var stiffnessScale = spring.Interface?.StiffnessScale ?? 1.0;
            var dampingScale = spring.Interface?.DampingScale ?? 1.0;
            var baseStiffness =
                material.YoungModulusPa
                * spring.EffectiveAreaM2
                / spring.RestLength
                * stiffnessScale;
            fractureDissipationEnergy +=
                0.5
                * baseStiffness
                * extension
                * extension
                * Math.Max(0.0, spring.Damage - damageBefore);
            if (spring.IsBroken)
                continue;
            var relativeVelocity = state.Velocity[b] - state.Velocity[a];
            var relativeAlongSpring = Vec3.Dot(relativeVelocity, direction);
            var constitutive = _constitutiveModel.Evaluate(
                spring,
                strain,
                relativeAlongSpring / spring.RestLength,
                dt
            );
            var stiffness =
                Math.Max(constitutive.TangentModulusPa, 0.0)
                * spring.EffectiveAreaM2
                / spring.RestLength
                * stiffnessScale;
            var effectiveMass =
                (mesh.Nodes[a].MassKg * mesh.Nodes[b].MassKg)
                / Math.Max(mesh.Nodes[a].MassKg + mesh.Nodes[b].MassKg, 1e-18);
            var criticalDamping = 2.0 * Math.Sqrt(Math.Max(stiffness * effectiveMass, 0.0));
            var damping = material.DampingRatio * criticalDamping * dampingScale;
            var damageScale = 1.0 - spring.Damage;
            var forceMagnitude =
                (
                    constitutive.StressPa * spring.EffectiveAreaM2 * stiffnessScale
                    + damping * relativeAlongSpring
                ) * damageScale;
            var force = direction * forceMagnitude;
            state.Force[a] += force;
            state.Force[b] -= force;
            var springVolume = spring.EffectiveAreaM2 * spring.RestLength;
            elasticEnergy += constitutive.ElasticEnergyDensityJPerM3 * springVolume * damageScale;
            plasticDissipationEnergy +=
                constitutive.PlasticDissipationIncrementJPerM3 * springVolume * damageScale;
            structuralDampingEnergy +=
                damping * relativeAlongSpring * relativeAlongSpring * damageScale * dt;
        }
        return new(
            elasticEnergy,
            structuralDampingEnergy,
            plasticDissipationEnergy,
            fractureDissipationEnergy
        );
    }

    private static double Integrate(
        SimulationMesh mesh,
        SimulationSettings settings,
        SimulationState state
    )
    {
        var dt = settings.TimeStepSeconds;
        var dampingScale = Math.Clamp(1.0 - settings.GlobalVelocityDamping, 0.0, 1.0);
        var dissipated = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (!state.IsActive[i])
                continue;
            if (state.IsFixed[i])
            {
                state.Position[i] = mesh.Nodes[i].RestPosition;
                state.Velocity[i] = Vec3.Zero;
                continue;
            }
            var acceleration = state.Force[i] / mesh.Nodes[i].MassKg;
            var undampedVelocity = state.Velocity[i] + acceleration * dt;
            state.Velocity[i] = undampedVelocity * dampingScale;
            dissipated +=
                0.5
                * mesh.Nodes[i].MassKg
                * (undampedVelocity.LengthSquared - state.Velocity[i].LengthSquared);
            state.Position[i] += state.Velocity[i] * dt;
        }
        return Math.Max(0.0, dissipated);
    }

    private static double IntegrateRigidBodies(
        IReadOnlyList<RigidBodyState> bodies,
        SimulationSettings settings
    )
    {
        var dt = settings.TimeStepSeconds;
        var angularDamping = Math.Clamp(1.0 - settings.AngularVelocityDamping, 0.0, 1.0);
        var dissipated = 0.0;
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
            var undampedAngularVelocity =
                body.AngularVelocityRadPerSec + worldAngularAcceleration * dt;
            var localUndamped = body.Orientation.InverseRotate(undampedAngularVelocity);
            var undampedAngularEnergy =
                0.5 * Vec3.Dot(localUndamped, body.Definition.InertiaBodyKgM2 * localUndamped);
            body.AngularVelocityRadPerSec = undampedAngularVelocity * angularDamping;
            var localDamped = body.Orientation.InverseRotate(body.AngularVelocityRadPerSec);
            var dampedAngularEnergy =
                0.5 * Vec3.Dot(localDamped, body.Definition.InertiaBodyKgM2 * localDamped);
            dissipated += Math.Max(0.0, undampedAngularEnergy - dampedAngularEnergy);
            body.Orientation = (
                QuaternionD.FromAngularVelocity(body.AngularVelocityRadPerSec, dt)
                * body.Orientation
            ).Normalized();
        }
        return dissipated;
    }

    private static double GravityWorkCoordinate(
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<RigidBodyState> bodies,
        IReadOnlyCollection<FragmentCoarseBodyState> fragments,
        Vec3 gravity
    )
    {
        if (gravity.LengthSquared <= 1e-24)
            return 0.0;
        var coordinate = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            if (state.IsActive[i])
                coordinate += mesh.Nodes[i].MassKg * Vec3.Dot(gravity, state.Position[i]);
        foreach (var body in bodies)
            coordinate += body.MassKg * Vec3.Dot(gravity, body.WorldCenter);
        foreach (var fragment in fragments)
            coordinate += fragment.MassKg * Vec3.Dot(gravity, fragment.CenterOfMass);
        return coordinate;
    }

    private static void ClearForces(SimulationState state) => Array.Fill(state.Force, Vec3.Zero);

    private static void ClearInactiveForces(SimulationState state)
    {
        for (var i = 0; i < state.Force.Length; i++)
            if (!state.IsActive[i])
                state.Force[i] = Vec3.Zero;
    }

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
            if (state.IsActive[i])
                state.Force[i] += settings.Gravity * mesh.Nodes[i].MassKg;
    }

    private static double CalculateTranslationalKineticEnergy(
        SimulationMesh mesh,
        SimulationState state,
        IReadOnlyList<RigidBodyState> bodies,
        IReadOnlyCollection<FragmentCoarseBodyState> fragments
    )
    {
        var energy = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            if (state.IsActive[i])
                energy += 0.5 * mesh.Nodes[i].MassKg * state.Velocity[i].LengthSquared;
        foreach (var body in bodies)
            energy += 0.5 * body.MassKg * body.Velocity.LengthSquared;
        foreach (var fragment in fragments)
            energy +=
                fragment.TranslationalKineticEnergyJ + fragment.UnresolvedInternalKineticEnergyJ;
        return energy;
    }

    private static double CalculateRotationalKineticEnergy(
        IReadOnlyList<RigidBodyState> bodies,
        IReadOnlyCollection<FragmentCoarseBodyState> fragments
    )
    {
        var energy = 0.0;
        foreach (var body in bodies)
        {
            var localOmega = body.Orientation.InverseRotate(body.AngularVelocityRadPerSec);
            var angularMomentumLocal = body.Definition.InertiaBodyKgM2 * localOmega;
            energy += 0.5 * Vec3.Dot(localOmega, angularMomentumLocal);
        }
        energy += fragments.Sum(x => x.RotationalKineticEnergyJ);
        return energy;
    }

    private static double CalculateMaxDisplacement(SimulationMesh mesh, SimulationState state)
    {
        var max = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
            if (state.IsActive[i])
                max = Math.Max(max, (state.Position[i] - mesh.Nodes[i].RestPosition).Length);
        return max;
    }

    private static SimulationFrame CreateFrame(
        SimulationMesh mesh,
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
        double frictionEnergy,
        double contactPotentialEnergy,
        double contactDampingEnergy,
        double structuralDampingEnergy,
        double globalDampingEnergy,
        double plasticDissipationEnergy,
        double fractureDissipationEnergy,
        double externalWork,
        double constraintWork,
        int secondaryFragmentContactCount,
        IFragmentTopologyModel fragmentTopologyModel,
        FragmentRepresentationPolicy fragmentRepresentationPolicy,
        IReadOnlyList<FragmentCoarseState> coarseStates,
        FragmentTransferAudit transferAudit,
        IReadOnlyList<CoarseFragmentContactMetrics> coarseContactMetrics,
        IReadOnlyList<FragmentLineageRecord> fragmentLineage
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
        var fragments = fragmentTopologyModel.Analyze(mesh, state);
        var representations = fragmentRepresentationPolicy.DecideAll(fragments).ToList();
        var representedIds = representations
            .Select(x => x.FragmentId)
            .ToHashSet(StringComparer.Ordinal);
        representations.AddRange(
            coarseStates
                .Where(x => representedIds.Add(x.Id))
                .Select(x => new FragmentRepresentationDecision(
                    x.Id,
                    x.Representation,
                    "Fragment is active in the independent coarse-body lifecycle."
                ))
        );
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
            frictionEnergy,
            contactPotentialEnergy,
            contactDampingEnergy,
            PlasticDissipationEnergyJ: plasticDissipationEnergy,
            FractureDissipationEnergyJ: fractureDissipationEnergy,
            ExternalWorkJ: externalWork,
            ConstraintWorkJ: constraintWork,
            StructuralDampingEnergyJ: structuralDampingEnergy,
            GlobalDampingEnergyJ: globalDampingEnergy,
            FragmentClusters: fragments,
            SecondaryFragmentContactCount: secondaryFragmentContactCount,
            FragmentRepresentations: representations,
            FragmentCoarseStates: coarseStates,
            FragmentTransferAudit: transferAudit,
            NodeActive: (bool[])state.IsActive.Clone(),
            CoarseFragmentContacts: coarseContactMetrics.ToArray(),
            FragmentLineage: fragmentLineage
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
