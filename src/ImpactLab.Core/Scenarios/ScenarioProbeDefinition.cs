using ImpactLab.Core.Mathematics;
using ImpactLab.Core.PostProcessing;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioProbeDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Probe";
    public ProbeQuantity Quantity { get; set; } = ProbeQuantity.DisplacementMagnitude;
    public string? PartId { get; set; }
    public bool SurfaceOnly { get; set; }
    public Vec3 SelectionCenter { get; set; } = Vec3.Zero;
    public Vec3 SelectionSize { get; set; } = new(1e9, 1e9, 1e9);
}
