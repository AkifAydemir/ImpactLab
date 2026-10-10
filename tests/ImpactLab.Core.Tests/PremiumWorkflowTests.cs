using ImpactLab.Core.Constraints;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Projects;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;
using ImpactLab.Core.Verification;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PremiumWorkflowTests
{
    [Fact]
    public void EngineeringMaterialLibraryHasValidatedNamedGrades()
    {
        Assert.True(MaterialLibrary.All.Count >= 15);
        Assert.Contains(MaterialLibrary.All, x => x.Id == "aluminium-7075-t6");
        Assert.Contains(MaterialLibrary.All, x => x.Id == "titanium-ti6al4v-grade5");
        Assert.All(MaterialLibrary.All, x => x.Validate());
        Assert.All(
            MaterialLibrary.All.Where(x => !x.Id.StartsWith("generic-", StringComparison.Ordinal)),
            x => Assert.True(x.EffectiveYieldStrengthPa > 0)
        );
    }

    [Fact]
    public void ImpactLabProjectEmbedsGeometryAndCustomMaterial()
    {
        var root = TemporaryDirectory();
        try
        {
            var mesh = LayeredMesh();
            var geometryPath = Path.Combine(root, "target.stl");
            BinaryStlWriter.Write(geometryPath, mesh);
            var material = new MaterialDefinition(
                "project-alloy",
                "Project Alloy",
                5_000,
                100e9,
                0.30,
                0.003,
                0.1,
                0.02,
                YieldStrengthPa: 300e6,
                UltimateTensileStrengthPa: 450e6,
                Standard: "Project certificate"
            );
            var scenario = ScenarioPresetLibrary.CreatePlateImpact();
            scenario.Parts[0].GeometrySource = ScenarioGeometrySourceKind.ImportedMesh;
            scenario.Parts[0].ImportedMeshPath = geometryPath;
            scenario.Parts[0].AssemblyId = "layer-stack";
            scenario.Parts[0].LayerIndex = 2;
            scenario.Parts[0].NominalLayerThicknessMeters = 0.012;
            scenario.Parts[0].MaterialId = material.Id;
            var impactorPath = Path.Combine(root, "impactor.obj");
            File.WriteAllText(impactorPath, "v 0 0 0\nv 1 0 0\nv 0 1 0\nf 1 2 3\n");
            scenario.RigidBodies[0].GeometrySource = ScenarioGeometrySourceKind.ImportedMesh;
            scenario.RigidBodies[0].ImportedMeshPath = impactorPath;
            var packagePath = Path.Combine(root, "portable.impactlab");

            ImpactLabProjectPackage.Save(packagePath, scenario, [new MaterialProfile(material)]);
            File.Delete(geometryPath);
            File.Delete(impactorPath);
            var loaded = ImpactLabProjectPackage.Load(packagePath, Path.Combine(root, "cache"));

            Assert.Equal("ImpactLab.Project", loaded.Manifest.Format);
            Assert.Single(loaded.Materials);
            Assert.True(File.Exists(loaded.Scenario.Parts[0].ImportedMeshPath));
            Assert.True(File.Exists(loaded.Scenario.RigidBodies[0].ImportedMeshPath));
            Assert.Equal("project-alloy", loaded.Scenario.Parts[0].MaterialId);
            Assert.Equal("layer-stack", loaded.Scenario.Parts[0].AssemblyId);
            Assert.Equal(2, loaded.Scenario.Parts[0].LayerIndex);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ScenarioClonePreservesCompleteEngineeringSetup()
    {
        var source = ScenarioPresetLibrary.CreatePlateImpact();
        source.Loads.Add(new ScenarioLoadDefinition { Name = "Pulse" });
        source.Boundaries.Add(new ScenarioBoundaryDefinition { Name = "Fixture" });
        source.Probes.Add(new ScenarioProbeDefinition { Name = "Center probe" });
        source.ThermalBoundaries.Add(
            new ScenarioThermalBoundaryDefinition { TemperatureKelvin = 350 }
        );
        source.ThermalSources.Add(new ScenarioThermalSourceDefinition { PowerWatts = 120 });
        source.Physics = new()
        {
            DamageModelId = "damage.strain-growth-v1",
            LatticeConstitutiveModelId = "constitutive.custom-lattice",
            FragmentTopologyModelId = "failure.custom-fragments",
            RigidContactModelId = "contact.custom-rigid",
            DeformableContactModelId = "contact.custom-deformable",
            AdditionalModelIds = ["thermal.test-model"],
        };

        var clone = ScenarioCloner.Clone(source);

        Assert.Single(clone.Loads);
        Assert.Single(clone.Boundaries);
        Assert.Single(clone.Probes);
        Assert.Single(clone.ThermalBoundaries);
        Assert.Single(clone.ThermalSources);
        Assert.Equal(source.Parts[0].MaterialId, clone.Parts[0].MaterialId);
        Assert.Equal("damage.strain-growth-v1", clone.Physics.DamageModelId);
        Assert.Equal("constitutive.custom-lattice", clone.Physics.LatticeConstitutiveModelId);
        Assert.Equal("failure.custom-fragments", clone.Physics.FragmentTopologyModelId);
        Assert.Equal("contact.custom-rigid", clone.Physics.RigidContactModelId);
        Assert.Equal("contact.custom-deformable", clone.Physics.DeformableContactModelId);
        Assert.Contains("thermal.test-model", clone.Physics.AdditionalModelIds);
    }

    [Fact]
    public void StlSurfaceCanBePartitionedAndRoundTripped()
    {
        var root = TemporaryDirectory();
        try
        {
            var layers = MeshLayerPartitioner.Split(LayeredMesh(), 2);
            Assert.Equal(2, layers.Count);
            var path = Path.Combine(root, "layer.stl");
            BinaryStlWriter.Write(path, layers[0].Mesh);
            var imported = new AutoStlMeshImporter().Import(path);
            Assert.Equal(layers[0].Mesh.TriangleCount, imported.TriangleCount);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void DetailedCsvContainsStressStrainAndSafetyFactor()
    {
        var root = TemporaryDirectory();
        try
        {
            var material = MaterialLibrary.Get("steel-s355jr");
            var nodes = new[]
            {
                new MeshNode(0, "p", 0, 0, 0, Vec3.Zero, 1, 1, material),
                new MeshNode(1, "p", 1, 0, 0, Vec3.UnitX, 1, 1, material),
            };
            var mesh = new SimulationMesh(
                nodes,
                [new MeshSpring("p", 0, 1, 1, 1, material)],
                1,
                new() { [("p", 0, 0, 0)] = 0, [("p", 1, 0, 0)] = 1 },
                [new("p", 0, 2, 0, 1, new(Vec3.Zero, Vec3.UnitX))]
            );
            var frame = new SimulationFrame(
                0.1,
                [Vec3.Zero, new Vec3(1.001, 0, 0)],
                [Vec3.Zero, Vec3.Zero],
                [0, 0],
                0,
                0,
                0,
                [],
                [],
                [],
                0,
                0,
                0,
                0,
                0
            );
            var result = new SimulationResult(
                [frame],
                new SimulationTelemetrySeries(),
                0.001,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                TimeSpan.Zero,
                new ConstraintReactionSeries()
            );
            var path = Path.Combine(root, "results.csv");
            ResultCsvExporter.Write(path, mesh, result);
            var csv = File.ReadAllText(path);
            Assert.Contains("equivalent_stress_pa", csv);
            Assert.Contains("yield_safety_factor", csv);
            Assert.Contains("steel-s355jr", csv);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void DefaultPlateUsesConservativeExplicitTimeStepEstimate()
    {
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        var compiled = ScenarioCompiler.Compile(scenario, new MaterialCatalog());
        var result = new SimulationResult(
            [],
            new SimulationTelemetrySeries(),
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            TimeSpan.Zero,
            new ConstraintReactionSeries()
        );

        var assessment = RunValidityAssessor.Assess(scenario, compiled, result, new(0, []));

        Assert.Equal(
            RunValidityStatus.Passed,
            assessment.Checks.Single(x => x.Id == "explicit-time-step").Status
        );
    }

    [Fact]
    public void EnergyLedgerClosesWhenContactDampingAccountsForLostMechanicalEnergy()
    {
        var telemetry = new SimulationTelemetrySeries();
        telemetry.Add(
            new SimulationTelemetrySample(
                TimeSeconds: 0,
                KineticEnergyJ: 100,
                RotationalKineticEnergyJ: 0,
                ElasticEnergyJ: 0,
                MaxDisplacementMeters: 0,
                MaxNodeDamage: 0,
                BrokenSpringCount: 0,
                ContactCount: 0,
                MaxContactPenetrationMeters: 0,
                TotalNormalContactForceN: 0,
                TotalTangentialContactForceN: 0,
                FrictionEnergyJ: 0,
                DeformableContactCount: 0
            )
        );
        telemetry.Add(
            new SimulationTelemetrySample(
                TimeSeconds: 0.1,
                KineticEnergyJ: 60,
                RotationalKineticEnergyJ: 0,
                ElasticEnergyJ: 0,
                MaxDisplacementMeters: 0,
                MaxNodeDamage: 0,
                BrokenSpringCount: 0,
                ContactCount: 1,
                MaxContactPenetrationMeters: 0.001,
                TotalNormalContactForceN: 1_000,
                TotalTangentialContactForceN: 0,
                FrictionEnergyJ: 10,
                DeformableContactCount: 0,
                ContactDampingEnergyJ: 30
            )
        );
        var result = new SimulationResult(
            [],
            telemetry,
            0,
            0,
            100,
            0,
            1,
            0.001,
            1_000,
            0,
            10,
            TimeSpan.Zero,
            new ConstraintReactionSeries()
        );

        var energy = EnergyBalanceAnalyzer.Analyze(result);
        var final = energy.Samples[^1];

        Assert.Equal(100, energy.ReferenceEnergyJ, 12);
        Assert.Equal(100, final.AccountedEnergyJ, 12);
        Assert.Equal(0, final.DiagnosticResidualJ, 12);
        Assert.Equal(0, final.ResidualPercent, 12);
    }

    [Fact]
    public void EnergyLedgerUsesCumulativeDampingAndIncrementalExternalWork()
    {
        var telemetry = new SimulationTelemetrySeries();
        telemetry.Add(EnergySample(time: 0, kinetic: 100, structuralDamping: 2, externalWork: 5));
        telemetry.Add(EnergySample(time: 0.1, kinetic: 83, structuralDamping: 22, externalWork: 8));
        var result = new SimulationResult(
            [],
            telemetry,
            0,
            0,
            100,
            0,
            0,
            0,
            0,
            0,
            0,
            TimeSpan.Zero,
            new ConstraintReactionSeries()
        );

        var energy = EnergyBalanceAnalyzer.Analyze(result);
        var final = energy.Samples[^1];

        Assert.Equal(102, energy.ReferenceEnergyJ, 12);
        Assert.Equal(105, final.AccountedEnergyJ, 12);
        Assert.Equal(0, final.DiagnosticResidualJ, 12);
    }

    private static SimulationTelemetrySample EnergySample(
        double time,
        double kinetic,
        double structuralDamping,
        double externalWork
    ) =>
        new(
            TimeSeconds: time,
            KineticEnergyJ: kinetic,
            RotationalKineticEnergyJ: 0,
            ElasticEnergyJ: 0,
            MaxDisplacementMeters: 0,
            MaxNodeDamage: 0,
            BrokenSpringCount: 0,
            ContactCount: 0,
            MaxContactPenetrationMeters: 0,
            TotalNormalContactForceN: 0,
            TotalTangentialContactForceN: 0,
            FrictionEnergyJ: 0,
            DeformableContactCount: 0,
            ExternalWorkJ: externalWork,
            StructuralDampingEnergyJ: structuralDamping
        );

    private static TriangleMeshAsset LayeredMesh() =>
        new(
            "layered",
            [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 1), new(0, 1, 1)],
            [0, 1, 2, 3, 4, 5]
        );

    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ImpactLab-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
