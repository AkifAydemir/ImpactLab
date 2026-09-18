using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Scenarios;

public static class ScenarioPresetLibrary
{
    public static IReadOnlyList<ScenarioDefinition> CreateDefaults() =>
        [CreatePlateImpact(), CreateAngleProfileImpact()];

    public static ScenarioDefinition CreatePlateImpact()
    {
        var scenario = new ScenarioDefinition
        {
            Name = "Plate impact study",
            CellSizeMeters = 0.03,
            Meshing = MeshingSettings.FromCellSize(0.03),
            Settings = new SimulationSettings
            {
                DurationSeconds = 0.003,
                TimeStepSeconds = 0.00002,
                SnapshotStride = 10,
                TelemetryStride = 5,
            },
        };
        var plate = new ScenarioPartDefinition
        {
            Id = "target-main",
            Name = "Primary plate",
            GeometryKind = GeometryKind.Plate,
            MaterialId = "generic-steel",
            FixMinimumZPlane = true,
        };
        plate.GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.Plate)
            .CreateDefaults();
        var sphere = new ScenarioRigidBodyDefinition
        {
            Id = "impactor-1",
            Name = "Rigid sphere",
            GeometryKind = GeometryKind.Sphere,
            MaterialId = "generic-steel",
            InitialVelocity = new Vec3(0, 0, -30),
        };
        sphere.GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.Sphere)
            .CreateDefaults();
        sphere.GeometryParameters.Set("centerZ", 0.095);
        sphere.GeometryParameters.Set("radius", 0.025);
        scenario.Parts.Add(plate);
        scenario.RigidBodies.Add(sphere);
        scenario.Contacts.Add(new("impactor-1", "target-main"));
        return scenario;
    }

    public static ScenarioDefinition CreateAngleProfileImpact()
    {
        var scenario = CreatePlateImpact();
        scenario.Id = Guid.NewGuid().ToString("N");
        scenario.Name = "Angle-profile comparison";
        scenario.Parts[0].GeometryKind = GeometryKind.AngleProfile;
        scenario.Parts[0].GeometryParameters = GeometryParameterSchemas
            .For(GeometryKind.AngleProfile)
            .CreateDefaults();
        return scenario;
    }
}
