using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Physics;
using ImpactLab.Core.Simulation;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FragmentClusterAnalyzerTests
{
    [Fact]
    public void BrokenConnectionCreatesMassAndMomentumConservingClusters()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "part", 0, 0, 0, Vec3.Zero, 2, 1, material),
            new MeshNode(1, "part", 1, 0, 0, Vec3.UnitX, 3, 1, material),
        };
        var spring = new MeshSpring("part", 0, 1, 1, 1, material);
        var mesh = new SimulationMesh(
            nodes,
            [spring],
            1,
            new() { [("part", 0, 0, 0)] = 0, [("part", 1, 0, 0)] = 1 },
            [new("part", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX))]
        );
        var state = new SimulationState(mesh);
        state.Velocity[0] = Vec3.UnitX;
        state.Velocity[1] = Vec3.UnitX * 3;

        var intact = FragmentClusterAnalyzer.Analyze(mesh, state);
        Assert.Single(intact);
        Assert.Empty(FragmentClusterAnalyzer.Detached(intact));

        spring.Damage = 1;
        var fragments = FragmentClusterAnalyzer.Analyze(mesh, state);

        Assert.Equal(2, fragments.Count);
        Assert.Equal(5, fragments.Sum(x => x.MassKg), 12);
        Assert.Equal(2, Assert.Single(FragmentClusterAnalyzer.Detached(fragments)).MassKg, 12);
        var momentum = fragments.Aggregate(
            Vec3.Zero,
            (sum, fragment) => sum + fragment.LinearMomentumKgMPerSec
        );
        Assert.Equal(new Vec3(11, 0, 0), momentum);
    }

    [Fact]
    public void ClusterAngularStateReconstructsAngularMomentum()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "rotor", 0, 0, 0, -Vec3.UnitX, 1, 1, material),
            new MeshNode(1, "rotor", 1, 0, 0, Vec3.UnitX, 1, 1, material),
        };
        var mesh = new SimulationMesh(
            nodes,
            [new MeshSpring("rotor", 0, 1, 2, 1, material)],
            1,
            new() { [("rotor", 0, 0, 0)] = 0, [("rotor", 1, 0, 0)] = 1 },
            [new("rotor", 0, 2, 0, 1, new(-Vec3.UnitX, Vec3.UnitX))]
        );
        var state = new SimulationState(mesh);
        state.Velocity[0] = -Vec3.UnitY * 2;
        state.Velocity[1] = Vec3.UnitY * 2;

        var fragment = Assert.Single(FragmentClusterAnalyzer.Analyze(mesh, state));
        var reconstructed = fragment.InertiaTensorKgM2 * fragment.AngularVelocityRadPerSec;

        Assert.Equal(4, fragment.AngularMomentumKgM2PerSec.Z, 12);
        Assert.Equal(fragment.AngularMomentumKgM2PerSec.X, reconstructed.X, 12);
        Assert.Equal(fragment.AngularMomentumKgM2PerSec.Y, reconstructed.Y, 12);
        Assert.Equal(fragment.AngularMomentumKgM2PerSec.Z, reconstructed.Z, 12);
        Assert.True(fragment.RotationalKineticEnergyJ > 0);
        Assert.Equal(fragment.TotalNodalKineticEnergyJ, fragment.ProjectedKineticEnergyJ, 12);

        var coarse = Assert.Single(
            FragmentStateTransfer.ProjectCoarseStates(
                [fragment],
                [new(fragment.Id, FragmentRepresentationKind.ReducedOrderRigid, "test projection")]
            )
        );
        Assert.Equal(fragment.MassKg, coarse.MassKg, 12);
        Assert.Equal(fragment.LinearMomentumKgMPerSec, coarse.LinearMomentumKgMPerSec);
        Assert.Equal(fragment.AngularMomentumKgM2PerSec, coarse.AngularMomentumKgM2PerSec);
        Assert.Equal(fragment.TotalNodalKineticEnergyJ, coarse.AccountedKineticEnergyJ, 12);
        var audit = FragmentStateTransfer.AuditCoarseProjection([fragment], [coarse]);
        Assert.True(audit.IsConservative());
        Assert.Equal(0, audit.MassResidualKg, 12);
        Assert.Equal(0, audit.LinearMomentumResidualKgMPerSec, 12);
        Assert.Equal(0, audit.IntrinsicAngularMomentumResidualKgM2PerSec, 12);
        Assert.Equal(0, audit.KineticEnergyResidualJ, 12);
    }

    [Fact]
    public void RepresentationPolicySelectsResolvedReducedAndStatisticalLevels()
    {
        var policy = new FragmentRepresentationPolicy(
            new FragmentRepresentationSettings
            {
                MinimumResolvedNodeCount = 4,
                MinimumReducedOrderNodeCount = 2,
                MinimumResolvedMassKg = 4,
                MinimumReducedOrderMassKg = 1,
            }
        );

        Assert.Equal(
            FragmentRepresentationKind.ResolvedDeformable,
            policy.Decide(Fragment("supported", 1, 0.1, true)).Kind
        );
        Assert.Equal(
            FragmentRepresentationKind.ResolvedDeformable,
            policy.Decide(Fragment("large", 4, 4, false)).Kind
        );
        Assert.Equal(
            FragmentRepresentationKind.ReducedOrderRigid,
            policy.Decide(Fragment("medium", 2, 1, false)).Kind
        );
        Assert.Equal(
            FragmentRepresentationKind.StatisticalParticle,
            policy.Decide(Fragment("small", 1, 0.1, false)).Kind
        );

        var topologyDecisions = policy.DecideAll([
            Fragment("primary", 1, 2, false),
            Fragment("detached", 1, 0.1, false),
        ]);
        Assert.Equal(
            FragmentRepresentationKind.ResolvedDeformable,
            topologyDecisions.Single(x => x.FragmentId == "primary").Kind
        );
        Assert.Equal(
            FragmentRepresentationKind.StatisticalParticle,
            topologyDecisions.Single(x => x.FragmentId == "detached").Kind
        );
    }

    [Fact]
    public void DetachedClustersReceiveSecondarySelfContact()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "part", 0, 0, 0, Vec3.Zero, 1, 1, material),
            new MeshNode(1, "part", 1, 0, 0, Vec3.UnitX * 0.5, 1, 1, material),
        };
        var spring = new MeshSpring("part", 0, 1, 0.5, 1, material) { Damage = 1 };
        var mesh = new SimulationMesh(
            nodes,
            [spring],
            1,
            new() { [("part", 0, 0, 0)] = 0, [("part", 1, 0, 0)] = 1 },
            [new("part", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX * 0.5))]
        );

        var result = new ExplicitLatticeSolver(new MaterialResponseDamageModel()).Run(
            mesh,
            new SimulationSettings
            {
                TimeStepSeconds = 1e-6,
                DurationSeconds = 2e-6,
                SnapshotStride = 1,
                TelemetryStride = 1,
                EnableDeformableContact = false,
                EnableFragmentSelfContact = true,
                GlobalVelocityDamping = 0,
                AngularVelocityDamping = 0,
            },
            [],
            [],
            contactSettings: new ContactSettings
            {
                NormalStiffnessNPerM = 1_000,
                NormalDampingNsPerM = 0,
                NodeRadiusScale = 0.48,
            }
        );

        Assert.Contains(result.Telemetry.Samples, x => x.FragmentClusterCount == 2);
        Assert.Contains(result.Telemetry.Samples, x => x.StatisticalFragmentCount == 1);
        Assert.Contains(result.Telemetry.Samples, x => x.SecondaryFragmentContactCount > 0);
        Assert.Contains(result.Frames, x => x.SecondaryFragmentContactCount > 0);
        Assert.Contains(
            result.Frames,
            x =>
                x.FragmentRepresentations?.Count(y =>
                    y.Kind == FragmentRepresentationKind.StatisticalParticle
                ) == 1
        );
        Assert.Contains(result.Frames, x => x.FragmentCoarseStates?.Count == 1);
        Assert.All(result.Frames, x => Assert.True(x.FragmentTransferAudit?.IsConservative()));
    }

    [Fact]
    public void EnabledCoarseIntegrationDeactivatesSourceNodesWithoutDuplicatingMass()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "part", 0, 0, 0, Vec3.Zero, 2, 1, material),
            new MeshNode(1, "part", 1, 0, 0, Vec3.UnitX, 1, 1, material),
        };
        var mesh = new SimulationMesh(
            nodes,
            [new MeshSpring("part", 0, 1, 1, 1, material) { Damage = 1 }],
            1,
            new() { [("part", 0, 0, 0)] = 0, [("part", 1, 0, 0)] = 1 },
            [new("part", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX))]
        );

        var result = new ExplicitLatticeSolver(new MaterialResponseDamageModel()).Run(
            mesh,
            new SimulationSettings
            {
                TimeStepSeconds = 1e-3,
                DurationSeconds = 2e-3,
                SnapshotStride = 1,
                TelemetryStride = 1,
                Gravity = new Vec3(0, 0, -10),
                EnableDeformableContact = false,
                EnableFragmentSelfContact = false,
                EnableCoarseFragmentIntegration = true,
                GlobalVelocityDamping = 0,
                AngularVelocityDamping = 0,
            },
            [],
            []
        );

        var final = result.Frames[^1];
        var coarse = Assert.Single(final.FragmentCoarseStates!);
        Assert.Equal("part:1", coarse.Id);
        Assert.True(coarse.CenterOfMass.Z < 0);
        Assert.Equal(0, final.Position[1].Z, 12);
        Assert.True(final.NodeActive![0]);
        Assert.False(final.NodeActive[1]);
        var lineage = final.FragmentLineage!.Single(x => x.FragmentId == "part:1");
        Assert.Null(lineage.ParentFragmentId);
        Assert.Equal(0, lineage.Generation);
        Assert.Contains(
            result.Telemetry.Samples,
            x => x.FragmentClusterCount == 2 && x.DetachedFragmentMassKg == 1
        );
    }

    [Fact]
    public void IntegratedCoarseResolvedContactPreservesSystemMomentum()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "part", 0, 0, 0, Vec3.Zero, 2, 1, material),
            new MeshNode(1, "part", 1, 0, 0, Vec3.UnitX * 0.8, 1, 1, material),
        };
        var mesh = new SimulationMesh(
            nodes,
            [new MeshSpring("part", 0, 1, 0.8, 1, material) { Damage = 1 }],
            1,
            new() { [("part", 0, 0, 0)] = 0, [("part", 1, 0, 0)] = 1 },
            [new("part", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX * 0.8))]
        );

        var result = new ExplicitLatticeSolver(new MaterialResponseDamageModel()).Run(
            mesh,
            new SimulationSettings
            {
                TimeStepSeconds = 1e-4,
                DurationSeconds = 1e-4,
                SnapshotStride = 1,
                TelemetryStride = 1,
                EnableDeformableContact = false,
                EnableFragmentSelfContact = false,
                EnableCoarseFragmentIntegration = true,
                GlobalVelocityDamping = 0,
                AngularVelocityDamping = 0,
            },
            [],
            [],
            contactSettings: new ContactSettings
            {
                NormalStiffnessNPerM = 1_000,
                NormalDampingNsPerM = 0,
                NodeRadiusScale = 0.5,
            }
        );

        var final = result.Frames[^1];
        var coarse = Assert.Single(final.FragmentCoarseStates!);
        var totalMomentum = final.Velocity[0] * nodes[0].MassKg + coarse.LinearMomentumKgMPerSec;
        Assert.Contains(result.Telemetry.Samples, x => x.CoarseFragmentContactCount > 0);
        Assert.Equal(0, totalMomentum.X, 12);
        Assert.Equal(0, totalMomentum.Y, 12);
        Assert.Equal(0, totalMomentum.Z, 12);
        Assert.All(
            final.CoarseFragmentContacts!,
            x => Assert.Equal(0, x.MomentumBalanceResidualN, 12)
        );
    }

    [Fact]
    public void SolverTimelineRecordsParentWhenAnIntactPartSplits()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var nodes = new[]
        {
            new MeshNode(0, "part", 0, 0, 0, Vec3.Zero, 2, 1, material),
            new MeshNode(1, "part", 1, 0, 0, Vec3.UnitX, 1, 1, material),
        };
        var mesh = new SimulationMesh(
            nodes,
            [new MeshSpring("part", 0, 1, 1, 1, material)],
            1,
            new() { [("part", 0, 0, 0)] = 0, [("part", 1, 0, 0)] = 1 },
            [new("part", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX))]
        );

        var result = new ExplicitLatticeSolver(new ImmediateFailureDamageModel()).Run(
            mesh,
            new SimulationSettings
            {
                TimeStepSeconds = 1e-6,
                DurationSeconds = 1e-6,
                SnapshotStride = 1,
                TelemetryStride = 1,
                EnableDeformableContact = false,
                EnableFragmentSelfContact = false,
                GlobalVelocityDamping = 0,
                AngularVelocityDamping = 0,
            },
            [],
            []
        );

        var child = result.Frames[^1].FragmentLineage!.Single(x => x.FragmentId == "part:1");
        Assert.Equal("part:0", child.ParentFragmentId);
        Assert.Equal(1, child.Generation);
        Assert.Contains(result.Telemetry.Samples, x => x.FragmentBirthCount == 1);
    }

    private static FragmentClusterSnapshot Fragment(
        string id,
        int nodeCount,
        double massKg,
        bool supported
    ) =>
        new(
            id,
            "part",
            Enumerable.Range(0, nodeCount).ToArray(),
            massKg,
            Vec3.Zero,
            Vec3.Zero,
            new(Vec3.Zero, Vec3.UnitX),
            supported,
            [],
            Matrix3.Identity,
            1,
            Vec3.Zero,
            Vec3.Zero,
            0,
            0,
            0,
            0
        );

    private sealed class ImmediateFailureDamageModel : IDamageModel
    {
        public double UpdateDamage(
            MeshSpring spring,
            double currentDamage,
            double strain,
            double timeStepSeconds
        ) => 1;
    }
}
