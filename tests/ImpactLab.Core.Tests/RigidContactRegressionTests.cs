using ImpactLab.Core.Contact;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Physics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Scene;
using ImpactLab.Core.Simulation;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class RigidContactRegressionTests
{
    [Fact]
    public void ScenarioCompilerHonorsRigidBodyMassOverride()
    {
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.RigidBodies[0].MassOverrideKg = 10;

        var compiled = ScenarioCompiler.Compile(scenario, new MaterialCatalog());

        Assert.Equal(10, compiled.RigidBodies[0].MassKg, 12);
    }

    [Fact]
    public void OffsetSphereIsSampledAboutItsWorldCenter()
    {
        var sphere = new SphereSpec("sphere", new Vec3(0, 0, 2), 0.5);
        var sample = GeometryContact.Sample(
            sphere,
            new RigidTransform(sphere.Center, QuaternionD.Identity),
            new Vec3(0, 0, 2.5)
        );

        Assert.Equal(0, sample.SignedDistanceMeters, 12);
        Assert.Equal(Vec3.UnitZ, sample.OutwardNormal);
    }

    [Fact]
    public void MovingSphereProducesContactAgainstTargetSurface()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var target = new ScenePart(
            "target",
            "target",
            new BoxSpec("target", Vec3.Zero, new Vec3(0.04, 0.04, 0.04)),
            material,
            ScenePartBehavior.Deformable
        );
        var scene = new SimulationScene();
        scene.Add(target);
        var mesh = SceneMeshCompiler.Compile(scene, 0.01).Mesh;
        var sphere = new SphereSpec("sphere", new Vec3(0, 0, 0.031), 0.015);
        var body = new RigidBodyDefinition(
            "sphere",
            "sphere",
            sphere,
            material,
            new Vec3(0, 0, -5)
        );
        var rules = new ContactInteractionTable();
        rules.Set(new ContactPairRule("sphere", "target"));

        var result = new ExplicitLatticeSolver(new StrainDamageModel()).Run(
            mesh,
            new SimulationSettings
            {
                TimeStepSeconds = 0.0000002,
                DurationSeconds = 0.0002,
                SnapshotStride = 50,
                TelemetryStride = 10,
            },
            [],
            [],
            [body],
            ContactSettings.Default,
            rules
        );

        Assert.True(result.PeakContactCount > 0);
        Assert.True(result.PeakContactForceN > 0);
        Assert.True(result.MaxDisplacementMeters > 0);
    }

    [Fact]
    public void RigidContactReportsPenaltyPotentialAndDashpotDissipation()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var node = new MeshNode(0, "target", 0, 0, 0, new Vec3(0, 0, 1.4), 1, 1, material);
        var bounds = new BoundingBox3(node.RestPosition, node.RestPosition);
        var part = new MeshPartRange("target", 0, 1, 0, 0, bounds);
        var mesh = new SimulationMesh([node], [], 1, new() { [("target", 0, 0, 0)] = 0 }, [part]);
        var state = new SimulationState(mesh);
        state.Velocity[0] = new Vec3(0, 0, -2);
        var sphere = new SphereSpec("sphere", Vec3.Zero, 1);
        var body = new RigidBodyState(
            new RigidBodyDefinition("sphere", "sphere", sphere, material, Vec3.Zero)
        );
        var settings = new ContactSettings
        {
            NormalStiffnessNPerM = 1_000,
            NormalDampingNsPerM = 10,
            NodeRadiusScale = 0.5,
        };

        var metrics = RigidContactSolver.Apply(
            mesh,
            state,
            body,
            part,
            [0],
            settings,
            new ContactPairRule("sphere", "target"),
            0.01
        );

        Assert.Equal(1, metrics.ContactCount);
        Assert.Equal(0.1, metrics.MaxPenetrationMeters, 12);
        Assert.Equal(5, metrics.StoredNormalPenaltyEnergyJ, 12);
        Assert.Equal(0.4, metrics.DissipatedNormalDampingEnergyJ, 12);
    }
}
