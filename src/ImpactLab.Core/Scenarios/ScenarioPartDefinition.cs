using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
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
    public LengthUnit ImportedMeshUnit { get; set; } = LengthUnit.Meter;
    public bool CenterImportedMesh { get; set; }
    public bool WeldImportedMesh { get; set; }
    public string MaterialId { get; set; } = "generic-steel";
    public ScenePartBehavior Behavior { get; set; } = ScenePartBehavior.Deformable;
    public bool FixMinimumZPlane { get; set; }
    public GeometryTransform3 Transform { get; set; } = GeometryTransform3.Identity;
    public string AssemblyId { get; set; } = "main-assembly";
    public ScenarioPartRole Role { get; set; } = ScenarioPartRole.Target;
    public int LayerIndex { get; set; }
    public double? NominalLayerThicknessMeters { get; set; }
    public bool IsVisible { get; set; } = true;
}
