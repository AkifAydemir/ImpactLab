using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Parameters;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioRigidBodyDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Rigid body";
    public GeometryKind GeometryKind { get; set; } = GeometryKind.Sphere;
    public ParameterSet GeometryParameters { get; set; } =
        GeometryParameterSchemas.For(GeometryKind.Sphere).CreateDefaults();
    public string MaterialId { get; set; } = "generic-steel";
    public Vec3 InitialVelocity { get; set; } = Vec3.Zero;
    public Vec3 InitialAngularVelocity { get; set; } = Vec3.Zero;
}
