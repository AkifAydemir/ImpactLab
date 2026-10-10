using System.IO;
using System.Text.Json;
using ImpactLab.App.Rendering;
using ImpactLab.App.Services;
using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.Projects;
using ImpactLab.Core.Reporting;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Showcase;

public static class Program
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.FirstOrDefault() == "--drop-test")
            return RunDropTest(args.Skip(1).FirstOrDefault());
        var outputDirectory = Path.GetFullPath(
            args.Length == 0 ? Path.Combine("artifacts", "showcase", "cube-impact") : args[0]
        );
        Directory.CreateDirectory(outputDirectory);
        var geometryPath = Path.Combine(outputDirectory, "cube-target-80mm.stl");
        BinaryStlWriter.Write(geometryPath, Cube(0.08));

        var scenario = Scenario(geometryPath);
        var materials = new MaterialCatalog();
        var runner = new SimulationRunnerService();
        Console.WriteLine("Running cube impact showcase...");
        var run = runner.Run(scenario, materials);

        ImpactLabProjectPackage.Save(
            Path.Combine(outputDirectory, "cube-impact.impactlab"),
            scenario
        );
        ResultCsvExporter.Write(
            Path.Combine(outputDirectory, "cube-impact-final-frame.csv"),
            run.Compiled.Mesh,
            run.Result
        );
        var report = EngineeringReportBuilder.BuildDetailed(
            scenario,
            run.Backend.DisplayName,
            run.BackendSummary,
            materials,
            run.Validity,
            energyBalance: run.EnergyBalance
        );
        new ReportExportService().Export(
            Path.Combine(outputDirectory, "cube-impact-report.html"),
            report
        );
        SimulationAnimationExporter.ExportGif(
            Path.Combine(outputDirectory, "cube-impact-displacement.gif"),
            run,
            HeatmapMode.Displacement,
            640,
            360,
            20
        );
        var peakContactFrame = run
            .Result.Frames.Select((frame, index) => (Frame: frame, Index: index))
            .MaxBy(x => x.Frame.TotalNormalContactForceN)
            .Index;
        SimulationAnimationExporter.ExportFramePng(
            Path.Combine(outputDirectory, "cube-impact-peak-contact.png"),
            run,
            peakContactFrame,
            heatmap: HeatmapMode.EquivalentStress,
            width: 960,
            height: 540
        );
        SimulationAnimationExporter.ExportGif(
            Path.Combine(outputDirectory, "cube-impact-stress.gif"),
            run,
            HeatmapMode.EquivalentStress,
            640,
            360,
            20
        );

        var summaryPath = Path.Combine(outputDirectory, "cube-impact-summary.json");
        File.WriteAllText(
            summaryPath,
            JsonSerializer.Serialize(
                new
                {
                    scenario = scenario.Name,
                    target = "80 mm closed S355JR cube STL",
                    impactor = "30 mm 42CrMo4 sphere",
                    impactSpeedMps = 30,
                    nodeCount = run.Compiled.Mesh.Nodes.Count,
                    springCount = run.Compiled.Mesh.Springs.Count,
                    frameCount = run.Result.Frames.Count,
                    run.Summary.MaxDisplacementMeters,
                    run.Summary.PeakContactForceN,
                    run.Summary.BrokenSpringCount,
                    finalFragmentClusters = run.Result.Frames.Last().FragmentClusters?.Count ?? 0,
                    detachedFragmentClusters = run.Result.Frames.Last().FragmentClusters
                        is { } cubeFragments
                        ? FragmentClusterAnalyzer.Detached(cubeFragments).Count
                        : 0,
                    finalEnergyResidualPercent = run.EnergyBalance.Samples.Last().ResidualPercent,
                    maxEnergyResidualPercent = run.EnergyBalance.Samples.Max(x =>
                        Math.Abs(x.ResidualPercent)
                    ),
                    validity = new
                    {
                        run.Validity.PassedCount,
                        run.Validity.WarningCount,
                        run.Validity.FailedCount,
                    },
                },
                Json
            )
        );
        Console.WriteLine($"Showcase written to {outputDirectory}");
        return 0;
    }

    private static int RunDropTest(string? requestedOutput)
    {
        var outputDirectory = Path.GetFullPath(
            requestedOutput ?? Path.Combine("artifacts", "showcase", "drop-test-10kg-10m")
        );
        Directory.CreateDirectory(outputDirectory);
        var scenario = DropTestScenario();
        var materials = new MaterialCatalog();
        Console.WriteLine("Running 10 kg / 10 m equivalent plate drop test...");
        var run = new SimulationRunnerService().Run(scenario, materials);
        const string prefix = "drop-test-10kg-10m";
        ImpactLabProjectPackage.Save(
            Path.Combine(outputDirectory, $"{prefix}.impactlab"),
            scenario
        );
        ResultCsvExporter.Write(
            Path.Combine(outputDirectory, $"{prefix}-final-frame.csv"),
            run.Compiled.Mesh,
            run.Result
        );
        new ReportExportService().Export(
            Path.Combine(outputDirectory, $"{prefix}-report.html"),
            EngineeringReportBuilder.BuildDetailed(
                scenario,
                run.Backend.DisplayName,
                run.BackendSummary,
                materials,
                run.Validity,
                energyBalance: run.EnergyBalance
            )
        );
        SimulationAnimationExporter.ExportGif(
            Path.Combine(outputDirectory, $"{prefix}-stress.gif"),
            run,
            HeatmapMode.EquivalentStress,
            640,
            360,
            20
        );
        var peakContactFrame = run
            .Result.Frames.Select((frame, index) => (Frame: frame, Index: index))
            .MaxBy(x => x.Frame.TotalNormalContactForceN)
            .Index;
        SimulationAnimationExporter.ExportFramePng(
            Path.Combine(outputDirectory, $"{prefix}-peak-contact.png"),
            run,
            peakContactFrame,
            HeatmapMode.EquivalentStress,
            960,
            540
        );

        var impactSpeed = Math.Sqrt(2 * 9.80665 * 10);
        var peakFrame = run.Result.Frames[peakContactFrame];
        var reboundSpeed = run
            .Result.Frames.Skip(peakContactFrame)
            .Select(x => x.RigidBodies[0].Velocity.Z)
            .DefaultIfEmpty(0)
            .Max();
        var reboundHeight = Math.Max(0, reboundSpeed * reboundSpeed / (2 * 9.80665));
        var coefficientOfRestitution = reboundSpeed / impactSpeed;
        var impactEnergy = 0.5 * 10 * impactSpeed * impactSpeed;
        var reboundEnergy = 0.5 * 10 * reboundSpeed * reboundSpeed;
        var energyCheck = run.Validity.Checks.Single(x => x.Id == "energy-balance");
        var finalFrame = run.Result.Frames[^1];
        var topCenterNode = run
            .Compiled.Mesh.Nodes.Select((node, index) => (Node: node, Index: index))
            .OrderByDescending(x => x.Node.RestPosition.Z)
            .ThenBy(x =>
                x.Node.RestPosition.X * x.Node.RestPosition.X
                + x.Node.RestPosition.Y * x.Node.RestPosition.Y
            )
            .First();
        var centerPermanentDeflection =
            finalFrame.Position[topCenterNode.Index].Z - topCenterNode.Node.RestPosition.Z;
        File.WriteAllText(
            Path.Combine(outputDirectory, $"{prefix}-summary.json"),
            JsonSerializer.Serialize(
                new
                {
                    scenario = scenario.Name,
                    assumptions = new
                    {
                        impactor = "10 kg solid steel sphere",
                        equivalentDropHeightM = 10,
                        impactSpeedMps = impactSpeed,
                        sphereDiameterMm = scenario.RigidBodies[0].GeometryParameters.Get("radius")
                            * 2000,
                        plate = "500 x 500 x 50 mm S355JR",
                        support = "four edge bands clamped",
                    },
                    mesh = new
                    {
                        cellSizeMm = scenario.CellSizeMeters * 1000,
                        nodeCount = run.Compiled.Mesh.Nodes.Count,
                        springCount = run.Compiled.Mesh.Springs.Count,
                        frameCount = run.Result.Frames.Count,
                    },
                    result = new
                    {
                        peakContactTimeMs = peakFrame.TimeSeconds * 1000,
                        peakContactForceN = run.Result.PeakContactForceN,
                        maxTargetDisplacementMm = run.Result.MaxDisplacementMeters * 1000,
                        centerFinalZChangeMm = centerPermanentDeflection * 1000,
                        run.Result.BrokenSpringCount,
                        finalFragmentClusters = finalFrame.FragmentClusters?.Count ?? 0,
                        detachedFragmentClusters = finalFrame.FragmentClusters is { } dropFragments
                            ? FragmentClusterAnalyzer.Detached(dropFragments).Count
                            : 0,
                        reboundSpeedMps = reboundSpeed,
                        equivalentReboundHeightM = reboundHeight,
                        coefficientOfRestitution,
                        impactEnergyJ = impactEnergy,
                        reboundTranslationalEnergyJ = reboundEnergy,
                    },
                    validity = new
                    {
                        classification = run.Validity.HasFailure ? "SCREENING_ONLY_NOT_VALIDATED"
                        : run.Validity.WarningCount > 0 ? "SCREENING_WITH_WARNINGS"
                        : "SCREENING_CHECKS_PASSED",
                        run.Validity.PassedCount,
                        run.Validity.WarningCount,
                        run.Validity.FailedCount,
                        maximumMechanicalEnergyResidualPercent = energyCheck.Observed,
                    },
                    limitation = "Coarse lattice screening model with two cells through plate thickness; force, stress, rebound and end-state displacement are not validated design values. Requires mesh convergence, calibrated plasticity/contact, complete dissipation accounting and physical-test correlation.",
                },
                Json
            )
        );
        if (run.Validity.HasFailure)
            Console.WriteLine(
                "WARNING: validity screening failed; treat the numerical values as qualitative only."
            );
        Console.WriteLine($"Drop-test showcase written to {outputDirectory}");
        return 0;
    }

    private static ScenarioDefinition DropTestScenario()
    {
        const double cellSize = 0.025;
        const double plateWidth = 0.5;
        const double plateThickness = 0.05;
        const double massKg = 10;
        const double density = 7850;
        const double dropHeight = 10;
        var sphereRadius = Math.Pow(3 * (massKg / density) / (4 * Math.PI), 1.0 / 3.0);
        var impactSpeed = Math.Sqrt(2 * 9.80665 * dropHeight);
        var scenario = new ScenarioDefinition
        {
            Name = "10 kg steel sphere / 10 m equivalent drop onto 50 mm S355 plate",
            CellSizeMeters = cellSize,
            Meshing = MeshingSettings.FromCellSize(cellSize),
            Settings = new SimulationSettings
            {
                DurationSeconds = 0.003,
                TimeStepSeconds = 0.000001,
                SnapshotStride = 60,
                TelemetryStride = 10,
                GlobalVelocityDamping = 0.00005,
                AngularVelocityDamping = 0,
            },
            ContactSettings = ContactSettings.Default with
            {
                NormalStiffnessNPerM = 3.0e8,
                NormalDampingNsPerM = 8.0e3,
                TangentialDampingNsPerM = 1.0e3,
                StaticFrictionCoefficient = 0.2,
                DynamicFrictionCoefficient = 0.15,
            },
        };
        var plate = new ScenarioPartDefinition
        {
            Id = "s355-plate",
            Name = "500 x 500 x 50 mm S355JR plate",
            GeometryKind = GeometryKind.Plate,
            MaterialId = "steel-s355jr",
            Role = ScenarioPartRole.Target,
        };
        plate.GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.Plate)
            .CreateDefaults();
        plate.GeometryParameters.Set("width", plateWidth);
        plate.GeometryParameters.Set("height", plateWidth);
        plate.GeometryParameters.Set("thickness", plateThickness);
        scenario.Parts.Add(plate);

        var edgeCenter = plateWidth * 0.5 - cellSize * 0.5;
        var edgeBand = cellSize * 1.1;
        scenario.Boundaries.Add(
            Clamp(
                "edge-x-min",
                new(-edgeCenter, 0, 0),
                new(edgeBand, plateWidth, plateThickness * 2)
            )
        );
        scenario.Boundaries.Add(
            Clamp(
                "edge-x-max",
                new(edgeCenter, 0, 0),
                new(edgeBand, plateWidth, plateThickness * 2)
            )
        );
        scenario.Boundaries.Add(
            Clamp(
                "edge-y-min",
                new(0, -edgeCenter, 0),
                new(plateWidth, edgeBand, plateThickness * 2)
            )
        );
        scenario.Boundaries.Add(
            Clamp(
                "edge-y-max",
                new(0, edgeCenter, 0),
                new(plateWidth, edgeBand, plateThickness * 2)
            )
        );

        var impactor = new ScenarioRigidBodyDefinition
        {
            Id = "drop-sphere",
            Name = "10 kg solid steel sphere",
            GeometryKind = GeometryKind.Sphere,
            MaterialId = "steel-42crmo4-qt",
            MassOverrideKg = massKg,
            InitialVelocity = new Vec3(0, 0, -impactSpeed),
        };
        impactor.GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.Sphere)
            .CreateDefaults();
        impactor.GeometryParameters.Set("centerX", 0);
        impactor.GeometryParameters.Set("centerY", 0);
        impactor.GeometryParameters.Set("centerZ", plateThickness * 0.5 + sphereRadius + 0.01);
        impactor.GeometryParameters.Set("radius", sphereRadius);
        scenario.RigidBodies.Add(impactor);
        scenario.Contacts.Add(new("drop-sphere", "s355-plate"));
        return scenario;
    }

    private static ScenarioBoundaryDefinition Clamp(string id, Vec3 center, Vec3 size) =>
        new()
        {
            Id = id,
            Name = $"Clamped {id}",
            Kind = ScenarioBoundaryKind.AxisLock,
            PartId = "s355-plate",
            SurfaceOnly = false,
            SelectionCenter = center,
            SelectionSize = size,
            AxisMask = AxisLockMask.All,
        };

    private static ScenarioDefinition Scenario(string geometryPath)
    {
        const double cellSize = 0.01;
        var scenario = new ScenarioDefinition
        {
            Name = "Cube STL spherical impact showcase",
            CellSizeMeters = cellSize,
            Meshing = MeshingSettings.FromCellSize(cellSize),
            Settings = new SimulationSettings
            {
                DurationSeconds = 0.001,
                TimeStepSeconds = 0.0000005,
                SnapshotStride = 40,
                TelemetryStride = 10,
                GlobalVelocityDamping = 0,
                AngularVelocityDamping = 0,
            },
            ContactSettings = ContactSettings.Default with
            {
                NormalStiffnessNPerM = 3.0e7,
                NormalDampingNsPerM = 0,
                TangentialDampingNsPerM = 0,
                StaticFrictionCoefficient = 0,
                DynamicFrictionCoefficient = 0,
            },
        };
        scenario.Parts.Add(
            new ScenarioPartDefinition
            {
                Id = "cube-target",
                Name = "80 mm cube target",
                GeometrySource = ScenarioGeometrySourceKind.ImportedMesh,
                ImportedMeshPath = geometryPath,
                ImportedMeshUnit = LengthUnit.Meter,
                MaterialId = "steel-s355jr",
                FixMinimumZPlane = false,
                Role = ScenarioPartRole.Target,
            }
        );
        var impactor = new ScenarioRigidBodyDefinition
        {
            Id = "sphere-impactor",
            Name = "30 mm spherical impactor",
            GeometryKind = GeometryKind.Sphere,
            MaterialId = "steel-42crmo4-qt",
            InitialVelocity = new Vec3(0, 0, -30),
        };
        impactor.GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.Sphere)
            .CreateDefaults();
        impactor.GeometryParameters.Set("centerX", 0);
        impactor.GeometryParameters.Set("centerY", 0);
        impactor.GeometryParameters.Set("centerZ", 0.065);
        impactor.GeometryParameters.Set("radius", 0.015);
        scenario.RigidBodies.Add(impactor);
        scenario.Contacts.Add(new("sphere-impactor", "cube-target"));
        return scenario;
    }

    private static TriangleMeshAsset Cube(double side)
    {
        var h = side * 0.5;
        return new(
            "cube-target-80mm",
            [
                new(-h, -h, -h),
                new(h, -h, -h),
                new(h, h, -h),
                new(-h, h, -h),
                new(-h, -h, h),
                new(h, -h, h),
                new(h, h, h),
                new(-h, h, h),
            ],
            [
                0,
                2,
                1,
                0,
                3,
                2,
                4,
                5,
                6,
                4,
                6,
                7,
                0,
                1,
                5,
                0,
                5,
                4,
                3,
                7,
                6,
                3,
                6,
                2,
                0,
                4,
                7,
                0,
                7,
                3,
                1,
                2,
                6,
                1,
                6,
                5,
            ]
        );
    }
}
