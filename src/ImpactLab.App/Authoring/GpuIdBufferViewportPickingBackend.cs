using ImpactLab.Core.Geometry;

namespace ImpactLab.App.Authoring;

public sealed class GpuIdBufferViewportPickingBackend : IViewportPickingBackend
{
    private readonly IGpuIdBufferReadback _readback;
    private readonly IReadOnlyDictionary<int, string> _ids;

    public GpuIdBufferViewportPickingBackend(
        IGpuIdBufferReadback readback,
        IReadOnlyDictionary<int, string> ids
    )
    {
        _readback = readback;
        _ids = ids;
    }

    public string Id => "gpu-id-buffer";
    public bool IsAvailable => _readback.IsAvailable;

    public ViewportPickHit? Pick(
        IEnumerable<(string Id, GeometrySpec Geometry)> parts,
        ViewportPickRequest request
    )
    {
        if (
            !IsAvailable
            || !_readback.TryReadObjectId(
                (int)Math.Round(request.ScreenX),
                (int)Math.Round(request.ScreenY),
                out var id
            )
            || !_ids.TryGetValue(id, out var objectId)
        )
            return null;
        return new(objectId, 0, request.Ray.Origin, Id);
    }
}
