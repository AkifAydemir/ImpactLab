using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scene;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Projects;

public static class ProjectMapper
{
    public static ProjectFileDto ToDto(SimulationProject project) =>
        new()
        {
            FormatVersion = project.FormatVersion,
            Name = project.Name,
            Parts = project.Scene.Parts.Select(ToDto).ToList(),
            RigidBodies = project.RigidBodies.Select(ToDto).ToList(),
            Settings = new ProjectSimulationSettingsDto
            {
                TimeStepSeconds = project.Settings.TimeStepSeconds,
                DurationSeconds = project.Settings.DurationSeconds,
                SnapshotStride = project.Settings.SnapshotStride,
                TelemetryStride = project.Settings.TelemetryStride,
            },
        };

    public static SimulationProject FromDto(ProjectFileDto dto)
    {
        if (dto.FormatVersion != 1)
            throw new NotSupportedException(
                $"Unsupported ImpactLab project format {dto.FormatVersion}."
            );
        var project = new SimulationProject
        {
            Name = dto.Name,
            Settings = new SimulationSettings
            {
                TimeStepSeconds = dto.Settings.TimeStepSeconds,
                DurationSeconds = dto.Settings.DurationSeconds,
                SnapshotStride = dto.Settings.SnapshotStride,
                TelemetryStride = dto.Settings.TelemetryStride,
            },
        };
        foreach (var part in dto.Parts)
            project.Scene.Add(FromDto(part));
        foreach (var body in dto.RigidBodies)
            project.RigidBodies.Add(FromDto(body));
        return project;
    }

    private static ProjectPartDto ToDto(ScenePart part) =>
        new()
        {
            Id = part.Id,
            Name = part.Name,
            Behavior = part.Behavior.ToString(),
            MaterialId = part.Material.Id,
            Geometry = GeometryToDto(part.Geometry),
        };

    private static ProjectRigidBodyDto ToDto(RigidBodyDefinition body) =>
        new()
        {
            Id = body.Id,
            Name = body.Name,
            MaterialId = body.Material.Id,
            Geometry = GeometryToDto(body.Geometry),
            Velocity = ToArray(body.InitialVelocity),
            AngularVelocity = ToArray(body.InitialAngularVelocityRadPerSec),
            MassOverrideKg = body.MassKg,
        };

    private static ScenePart FromDto(ProjectPartDto dto) =>
        new(
            dto.Id,
            dto.Name,
            GeometryFromDto(dto.Geometry),
            MaterialLibrary.Get(dto.MaterialId),
            Enum.Parse<ScenePartBehavior>(dto.Behavior, true)
        );

    private static RigidBodyDefinition FromDto(ProjectRigidBodyDto dto) =>
        new(
            dto.Id,
            dto.Name,
            GeometryFromDto(dto.Geometry),
            MaterialLibrary.Get(dto.MaterialId),
            ToVec(dto.Velocity),
            dto.MassOverrideKg,
            false,
            ToVec(dto.AngularVelocity)
        );

    private static ProjectGeometryDto GeometryToDto(GeometrySpec geometry)
    {
        var dto = new ProjectGeometryDto
        {
            Kind = geometry.Kind.ToString(),
            Name = geometry.Name,
            Center = ToArray(geometry.Center),
        };
        var p = dto.Parameters;
        switch (geometry)
        {
            case RectangularPlateSpec plate:
                p["width"] = plate.Width;
                p["height"] = plate.Height;
                p["thickness"] = plate.Thickness;
                break;
            case BoxSpec box:
                p["x"] = box.Size.X;
                p["y"] = box.Size.Y;
                p["z"] = box.Size.Z;
                break;
            case SphereSpec sphere:
                p["radius"] = sphere.Radius;
                break;
            case CylinderSpec cylinder:
                p["radius"] = cylinder.Radius;
                p["length"] = cylinder.Length;
                break;
            case TubeSpec tube:
                p["inner"] = tube.InnerRadius;
                p["outer"] = tube.OuterRadius;
                p["length"] = tube.Length;
                break;
            case AngleProfileSpec angle:
                p["legX"] = angle.LegX;
                p["legY"] = angle.LegY;
                p["thickness"] = angle.Thickness;
                p["length"] = angle.Length;
                break;
            default:
                throw new NotSupportedException(
                    $"Project serialization does not support {geometry.GetType().Name}."
                );
        }
        return dto;
    }

    private static GeometrySpec GeometryFromDto(ProjectGeometryDto dto)
    {
        var c = ToVec(dto.Center);
        var p = dto.Parameters;
        return dto.Kind.ToLowerInvariant() switch
        {
            "plate" => new RectangularPlateSpec(
                dto.Name,
                c,
                p["width"],
                p["height"],
                p["thickness"]
            ),
            "box" => new BoxSpec(dto.Name, c, new Vec3(p["x"], p["y"], p["z"])),
            "sphere" => new SphereSpec(dto.Name, c, p["radius"]),
            "cylinder" => new CylinderSpec(dto.Name, c, p["radius"], p["length"]),
            "tube" => new TubeSpec(dto.Name, c, p["inner"], p["outer"], p["length"]),
            "angleprofile" => new AngleProfileSpec(
                dto.Name,
                c,
                p["legX"],
                p["legY"],
                p["thickness"],
                p["length"]
            ),
            _ => throw new NotSupportedException($"Unknown geometry kind '{dto.Kind}'."),
        };
    }

    private static double[] ToArray(in Vec3 v) => [v.X, v.Y, v.Z];

    private static Vec3 ToVec(IReadOnlyList<double> v) => new(v[0], v[1], v[2]);
}
