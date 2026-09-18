using ImpactLab.Core.Geometry;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.Scene;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioPartDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Part";
    public ScenarioGeometrySourceKind GeometrySource { get; set; } =
        ScenarioGeometrySourceKind.Parametric;
    public GeometryKind GeometryKind { get; set; } = GeometryKind.Plate;
    public ParameterSet GeometryParameters { get; set; } =
        GeometryParameterSchemas.For(GeometryKind.Plate).CreateDefaults();
    public string? ImportedMeshPath { get; set; }
    public string MaterialId { get; set; } = "generic-steel";
    public ScenePartBehavior Behavior { get; set; } = ScenePartBehavior.Deformable;
    public bool FixMinimumZPlane { get; set; }
    public GeometryTransform3 Transform { get; set; } = GeometryTransform3.Identity;
}
