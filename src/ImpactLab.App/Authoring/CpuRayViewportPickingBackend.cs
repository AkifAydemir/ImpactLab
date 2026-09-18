using ImpactLab.Core.Authoring;
using ImpactLab.Core.Geometry;

namespace ImpactLab.App.Authoring;

public sealed class CpuRayViewportPickingBackend : IViewportPickingBackend
{
    private readonly AuthoringSelectionService _picker = new();
    public string Id => "cpu-ray";
    public bool IsAvailable => true;

    public ViewportPickHit? Pick(
        IEnumerable<(string Id, GeometrySpec Geometry)> parts,
        ViewportPickRequest request
    )
    {
        var h = _picker.Pick(parts, request.Ray);
        return h is null ? null : new(h.Value.PartId, h.Value.Distance, h.Value.Point, Id);
    }
}
