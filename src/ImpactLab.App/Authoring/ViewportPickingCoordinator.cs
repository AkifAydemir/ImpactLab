using ImpactLab.App.Accessibility;
using ImpactLab.Core.Geometry;

namespace ImpactLab.App.Authoring;

public sealed class ViewportPickingCoordinator
{
    private readonly SelectionService _selection;
    private readonly AccessibilityAnnouncementService _a11y;
    private readonly IReadOnlyList<IViewportPickingBackend> _backends;

    public ViewportPickingCoordinator(
        SelectionService selection,
        AccessibilityAnnouncementService a11y,
        params IViewportPickingBackend[] backends
    )
    {
        _selection = selection;
        _a11y = a11y;
        _backends = backends.Length == 0 ? [new CpuRayViewportPickingBackend()] : backends;
    }

    public ViewportPickHit? Pick(
        IEnumerable<(string Id, GeometrySpec Geometry)> parts,
        ViewportPickRequest request
    )
    {
        var hit = _backends
            .Where(x => x.IsAvailable)
            .Select(x => x.Pick(parts, request))
            .FirstOrDefault(x => x is not null);
        if (hit is null)
        {
            if (!request.Additive && !request.Toggle)
                _selection.Clear();
            _a11y.Announce("Viewport selection cleared.");
            return null;
        }
        if (request.Toggle)
            _selection.Select(hit.ObjectId, true);
        else
            _selection.Select(hit.ObjectId, request.Additive);
        _a11y.Announce($"Selected {hit.ObjectId} using {hit.Backend} picking.");
        return hit;
    }
}
