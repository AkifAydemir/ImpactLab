using ImpactLab.Core.Constraints;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioBoundaryDefinition
{
    public bool Enabled { get; set; } = true;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Boundary";
    public ScenarioBoundaryKind Kind { get; set; } = ScenarioBoundaryKind.AxisLock;
    public string? PartId { get; set; }
    public bool SurfaceOnly { get; set; }
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
    public bool LockX { get; set; } = true;
    public bool LockY { get; set; } = true;
    public bool LockZ { get; set; } = true;
    public AxisLockMask AxisMask { get; set; } = AxisLockMask.All;
    public Vec3 Vector { get; set; } = Vec3.Zero;
    public List<LoadCurvePoint> Curve { get; } = [];
}
