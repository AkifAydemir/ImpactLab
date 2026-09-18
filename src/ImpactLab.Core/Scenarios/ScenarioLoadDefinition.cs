using ImpactLab.Core.Loads;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioLoadDefinition
{
    public bool Enabled { get; set; } = true;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Load";
    public ScenarioLoadKind Kind { get; set; } = ScenarioLoadKind.PatchPressure;
    public string? PartId { get; set; }
    public bool SurfaceOnly { get; set; } = true;
    public Vec3 SelectionCenter { get; set; } = Vec3.Zero;
    public Vec3 SelectionSize { get; set; } = new(1e9, 1e9, 1e9);
    public Vec3 Center
    {
        get => SelectionCenter;
        set => SelectionCenter = value;
    }
    public Vec3 Size
    {
        get => SelectionSize;
        set => SelectionSize = value;
    }
    public Vec3 Direction { get; set; } = -Vec3.UnitZ;
    public double Magnitude { get; set; } = 1.0e6;
    public double DurationSeconds { get; set; } = 0.001;
    public Vec3 FieldCenter { get; set; } = Vec3.Zero;
    public double FieldRadiusMeters { get; set; } = 0.10;
    public double FieldSigmaMeters { get; set; } = 0.03;
    public List<LoadCurvePoint> Curve { get; } = [];
}
